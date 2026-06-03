using System.Runtime.CompilerServices;
using Mapster;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
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

            // Extract token usage from metadata if available
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

            // Extract token usage from metadata if available
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

    private static void ExtractTokenUsage(ChatMessageContent content, MessagePartResponse response)
    {
        if (content.Metadata is null)
        {
            return;
        }

        // Try to extract usage from Semantic Kernel's standard metadata
        if (content.Metadata.TryGetValue("Usage", out var usageObj))
        {
            if (usageObj is ChatTokenUsage usage)
            {
                response.PromptTokens = usage.InputTokenCount;
                response.CompletionTokens = usage.OutputTokenCount;
                response.TotalTokens = usage.TotalTokenCount;
            }
        }
    }
}
