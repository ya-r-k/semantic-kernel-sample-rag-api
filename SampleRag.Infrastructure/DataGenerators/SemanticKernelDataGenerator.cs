using System.Runtime.CompilerServices;
using Mapster;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
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

        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(chat, settingsFactory.GetSettings(executionSettingsName, outerArguments), kernel, cancellationToken: ct))
        {
            var result = content.Adapt<MessagePartResponse>();
            if (content.Role.Equals(AuthorRole.Tool))
            {
                result = chat.Adapt<MessagePartResponse>();
            }

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

            yield return result;
        }
    }
}
