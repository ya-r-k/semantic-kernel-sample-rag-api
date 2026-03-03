using System.Runtime.CompilerServices;
using Mapster;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Factories;
using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Enums;
using OllamaChatResponseStream = OllamaSharp.Models.Chat.ChatResponseStream;

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

    public async IAsyncEnumerable<MessagePart> GenerateStreamingData(IEnumerable<Message> messages, string executionSettingsName, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chat = messages.Adapt<ChatHistory>();

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        var prevGenerationStep = GenerationStep.Unknown;

        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(chat, settingsFactory.GetSettings(executionSettingsName), kernel, cancellationToken: ct))
        {
            var result = content.Adapt<MessagePart>();

            if (content.Role == AuthorRole.Tool)
            {
                /*result = new MessagePart
                {
                    ChunksIds = chat.Select(x => x.InnerContent as ChatMessageContentItemCollection)
                        .Where(x => x != null)
                        .SelectMany(x => x)
                        .Select(x => x.InnerContent as FunctionResultContent)
                        .Select(x => x.Result),
                };*/

                /*foreach (var item in content.Items)
                {
                    if (item is StreamingToolResultContent toolResult)
                    {
                        result.Text = $"🛠️ Tool Result: {toolResult.Result}\n" +
                                     $"Plugin: {toolResult.FunctionCall?.Name}\n" +
                                     $"Args: {toolResult.FunctionCall?.Arguments}";
                        result.Step = GenerationStep.ToolResult;  // Отдельный шаг для результатов
                        yield return result;
                        continue;
                    }
                }*/

                result.Text = $"FUNCTION CALLS";

                continue;
            }
            else if (content.Role == AuthorRole.Assistant && content.Content is null && 
                content.InnerContent is OllamaChatResponseStream innerContent &&
                innerContent?.Message?.Thinking is null)
            {
                result.Text = $"EMPTY AI TEXT";
            }

            if (result.Step == prevGenerationStep)
            {
                result.Step = GenerationStep.Unknown;
            }
            else
            {
                prevGenerationStep = result.Step;
            }

            yield return result;
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamingData(IEnumerable<Message> messages, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chat = messages.Adapt<ChatHistory>();

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(chat, cancellationToken: ct))
        {
            if (content?.Content is not null)
            {
                yield return content.Content;
            }
        }
    }
}
