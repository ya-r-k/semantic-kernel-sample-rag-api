using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Factories;
using SampleRag.Domain.Models;

namespace SampleRag.Application.DataGenerators;

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

    public async IAsyncEnumerable<string> GenerateStreamingData(IEnumerable<Message> messages, string executionSettingsName, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chat = BuildChatHistory(messages);

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(chat, settingsFactory.GetSettings(executionSettingsName), kernel, cancellationToken: ct))
        {
            if (content?.Content is not null)
            {
                yield return content.Content;
            }
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamingData(IEnumerable<Message> messages, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chat = BuildChatHistory(messages);

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        await foreach (var content in chatCompletion.GetStreamingChatMessageContentsAsync(chat, cancellationToken: ct))
        {
            if (content?.Content is not null)
            {
                yield return content.Content;
            }
        }
    }

    private ChatHistory BuildChatHistory(IEnumerable<Message> messages)
    {
        var chat = new ChatHistory();
        foreach (var message in messages)
        {
            chat.AddMessage(message.AiGenerated switch
            {
                true => AuthorRole.Assistant,
                false => AuthorRole.User,
            }, message.Text);
        }

        return chat;
    }
}
