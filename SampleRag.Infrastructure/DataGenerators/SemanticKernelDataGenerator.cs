using System.Runtime.CompilerServices;
using System.Text.Json;
using Mapster;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using OllamaChatDoneResponseStream = OllamaSharp.Models.Chat.ChatDoneResponseStream;
using SampleRag.Application.Filters.Invocation;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Factories;
using SampleRag.Domain.Models;

namespace SampleRag.Infrastructure.DataGenerators;

public class SemanticKernelDataGenerator(
    ISettingsFactory<PromptExecutionSettings> settingsFactory,
    Kernel kernel) : IDataGenerator
{
    public async IAsyncEnumerable<string> GenerateStreamingData(string message, string executionSettingsName, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var textGenerator = kernel.GetRequiredService<ITextGenerationService>();
        await foreach (var content in textGenerator.GetStreamingTextContentsAsync(message, settingsFactory.GetSettings(executionSettingsName), kernel, cancellationToken: ct))
        {
            if (content?.Text is not null)
            {
                yield return content.Text;
            }
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamingData(string message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var textGenerator = kernel.GetRequiredService<ITextGenerationService>();
        await foreach (var content in textGenerator.GetStreamingTextContentsAsync(message, cancellationToken: ct))
        {
            if (content?.Text is not null)
            {
                yield return content.Text;
            }
        }
    }

    public async IAsyncEnumerable<MessagePartResponse> GenerateStreamingData(IEnumerable<Message> messages, string executionSettingsName, IDictionary<string, object>? outerArguments = default, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chat = messages.Adapt<ChatHistory>();
        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        this.AddFilters(outerArguments);

        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(chat, settingsFactory.GetSettings(executionSettingsName, outerArguments), kernel, cancellationToken: ct))
        {
            var result = content.Adapt<MessagePartResponse>();
            if (content.Role.Equals(AuthorRole.Tool))
            {
                result = chat.Adapt<MessagePartResponse>();
            }

            ExtractTokenUsage(content, result);
            yield return result;
        }
    }

    public async IAsyncEnumerable<MessagePartResponse> GenerateStreamingData(IEnumerable<Message> messages, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chat = messages.Adapt<ChatHistory>();
        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(chat, cancellationToken: ct))
        {
            var result = content.Adapt<MessagePartResponse>();
            if (content.Role == AuthorRole.Tool)
            {
                result = chat.Adapt<MessagePartResponse>();
            }

            ExtractTokenUsage(content, result);
            yield return result;
        }
    }

    private void AddFilters(IDictionary<string, object>? outerArguments = default)
    {
        if (outerArguments is not null && outerArguments.Count > 0)
        {
            kernel.FunctionInvocationFilters.Add(new NonAiArgumentsApplyingFilter(outerArguments));
        }
    }

    private static void ExtractTokenUsage(StreamingChatMessageContent content, MessagePartResponse response)
    {
        if (content.Metadata is not null)
        {
            if (TryGetUsage(content.Metadata, out var promptTokens, out var completionTokens, out var totalTokens))
            {
                response.PromptTokens = promptTokens;
                response.CompletionTokens = completionTokens;
                response.TotalTokens = totalTokens ?? ((promptTokens ?? 0) + (completionTokens ?? 0));
                return;
            }
        }

        if (content.InnerContent is not null && TryGetUsage(content.InnerContent, out var innerPrompt, out var innerCompletion, out var innerTotal))
        {
            response.PromptTokens = innerPrompt;
            response.CompletionTokens = innerCompletion;
            response.TotalTokens = innerTotal ?? ((innerPrompt ?? 0) + (innerCompletion ?? 0));
        }
    }

    private static bool TryGetUsage(object value, out int? promptTokens, out int? completionTokens, out int? totalTokens)
    {
        promptTokens = null;
        completionTokens = null;
        totalTokens = null;

        if (value is JsonElement element)
        {
            if (TryGetIntFromJson(element, "InputTokenCount", out promptTokens) || TryGetIntFromJson(element, "PromptEvalCount", out promptTokens))
            {
            }
            TryGetIntFromJson(element, "OutputTokenCount", out completionTokens);
            TryGetIntFromJson(element, "EvalCount", out completionTokens);
            TryGetIntFromJson(element, "TotalTokenCount", out totalTokens);
            TryGetIntFromJson(element, "TotalEvalCount", out totalTokens);
            return promptTokens.HasValue || completionTokens.HasValue || totalTokens.HasValue;
        }

        if (value is IDictionary<string, object> dict)
        {
            if (TryGetIntFromDictionary(dict, out promptTokens, "InputTokenCount", "PromptEvalCount", "promptTokens", "prompt_eval_count")) { }
            if (TryGetIntFromDictionary(dict, out completionTokens, "OutputTokenCount", "EvalCount", "completionTokens", "completion_count", "eval_count")) { }
            if (TryGetIntFromDictionary(dict, out totalTokens, "TotalTokenCount", "TotalEvalCount", "totalTokens", "total_token_count")) { }
            return promptTokens.HasValue || completionTokens.HasValue || totalTokens.HasValue;
        }

        var type = value.GetType();
        promptTokens = TryGetPropertyValue(type, value, "InputTokenCount", "PromptEvalCount", "promptTokens", "prompt_eval_count");
        completionTokens = TryGetPropertyValue(type, value, "OutputTokenCount", "EvalCount", "completionTokens", "completion_count", "eval_count");
        totalTokens = TryGetPropertyValue(type, value, "TotalTokenCount", "TotalEvalCount", "totalTokens", "total_token_count");

        return promptTokens.HasValue || completionTokens.HasValue || totalTokens.HasValue;
    }

    private static bool TryGetIntFromJson(JsonElement element, string propertyName, out int? value)
    {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
        {
            value = number;
            return true;
        }

        return false;
    }

    private static bool TryGetIntFromDictionary(IDictionary<string, object> dict, out int? value, params string[] names)
    {
        value = null;
        foreach (var name in names)
        {
            if (!dict.TryGetValue(name, out var item))
            {
                continue;
            }

            if (item is int intValue)
            {
                value = intValue;
                return true;
            }

            if (item is long longValue)
            {
                value = (int)longValue;
                return true;
            }

            if (item is string str && int.TryParse(str, out var parsed))
            {
                value = parsed;
                return true;
            }

            if (item is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var jsonNumber))
            {
                value = jsonNumber;
                return true;
            }
        }

        return false;
    }

    private static int? TryGetPropertyValue(Type type, object instance, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(propertyName);
            if (property is null)
            {
                continue;
            }

            var rawValue = property.GetValue(instance);
            if (rawValue is int intValue)
            {
                return intValue;
            }

            if (rawValue is long longValue)
            {
                return (int)longValue;
            }

            if (rawValue is string str && int.TryParse(str, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }
}
