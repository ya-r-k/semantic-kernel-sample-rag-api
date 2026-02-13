using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Domain.Models;
using System.Runtime.CompilerServices;

namespace SampleRag.Application.Services;

public class DataGenerator(Kernel kernel) : IDataGenerator
{
    public async IAsyncEnumerable<string> GenerateStreamingData(string message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var generationService = kernel.GetRequiredService<ITextGenerationService>();

        await foreach (var content in generationService.GetStreamingTextContentsAsync(message, cancellationToken: ct))
        {
            if (string.IsNullOrEmpty(content.Text))
            {
                yield return content.Text;
            }
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamingData(IEnumerable<MessageData> messages, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chat = BuildChatHistory(messages);

        await foreach (var content in chatService.GetStreamingChatMessageContentsAsync(chat, cancellationToken: ct))
        {
            if (string.IsNullOrEmpty(content.Content))
            {
                yield return content.Content;
            }
        }
    }

    private ChatHistory BuildChatHistory(IEnumerable<MessageData> messages)
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
