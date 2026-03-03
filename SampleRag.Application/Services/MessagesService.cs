using System.Linq.Expressions;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class MessagesService(
    IChatService chatService,
    IDataGenerator dataGenerator,
    IRepository<Guid, Message> messagesRepository) : IMessagesService
{
    public async IAsyncEnumerable<MessagePart> GenerateAiResponce(SendMessageRequest request, string userId)
    {
        var userMessage = new Message
        {
            Text = request.Text,
            ChatId = request.ChatId ?? Guid.Empty,
        };

        var result = new List<IAsyncEnumerable<MessagePart>>();
        if (!userMessage.ChatId.Equals(Guid.Empty))
        {
            result.Add(chatService.StartNewChat(userMessage));
        }

        result.Add(GenerateAiMessage(userMessage));

        foreach (var parts in result)
        {
            await foreach (var part in parts)
            {
                yield return part;
            }
        }
    }

    public async Task<IEnumerable<Message>> GetBatchByAsync(Expression<Func<Message, bool>> expression, int batchSize)
    {
        return await messagesRepository.GetBatchByAsync(expression, batchSize);
    }

    private async IAsyncEnumerable<MessagePart> GenerateAiMessage(Message userMessage)
    {
        userMessage.CreatedAt = DateTime.UtcNow;
        var messagesHistory = await messagesRepository.GetBatchByAsync(x => x.ChatId.Equals(userMessage.ChatId), 30);

        var aiMessage = new Message
        {
            AiGenerated = true,
            ChatId = userMessage.ChatId,
            Text = string.Empty,
        };

        await foreach (var part in dataGenerator.GenerateStreamingData(messagesHistory.Append(userMessage), "naive-rag"))
        {
            aiMessage.Text += part.Text;
            yield return part;
        }

        aiMessage.CreatedAt = DateTime.UtcNow;

        await messagesRepository.AddAsync([userMessage, aiMessage]);

        yield return new MessagePart { CreatedAt = aiMessage.CreatedAt };
    }
}
