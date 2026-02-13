using SampleRag.Application.Interfaces;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Domain.Models;

namespace SampleRag.Application.Services;

public class MessageService(
    IRepository<int, MessageData> repository,
    IDataGenerator dataGenerator) : IMessageService
{
    public async IAsyncEnumerable<MessagePart> GenerateAiResponce(MessageData message, int historyWindow = 30)
    {
        var messagesHistory = await repository.GetBatchByAsync(x => x.ChatId.Equals(message.ChatId), historyWindow);
        var aiMessage = new MessageData
        {
            AiGenerated = true,
            ChatId = message.ChatId,
            Text = message.Text,
        };

        await foreach (var part in dataGenerator.GenerateStreamingData(messagesHistory.Append(message)))
        {
            aiMessage.Text += part;
            yield return new MessagePart { Text = part };
        }
        aiMessage.CreatedAt = DateTime.UtcNow;

        await AddAsync(aiMessage);

        yield return new MessagePart { CreatedAt = aiMessage.CreatedAt };
    }

    public async Task<IEnumerable<MessageData>> AddAsync(params MessageData[] items)
    {
        return await repository.AddAsync(items);
    }

    public async Task<IEnumerable<MessageData>> GetByIdsAsync(params int[] ids)
    {
        return await repository.GetByIdsAsync(ids);
    }

    public async Task RemoveByIdsAsync(params int[] ids)
    {
        await repository.RemoveByIdsAsync(ids);
    }
}
