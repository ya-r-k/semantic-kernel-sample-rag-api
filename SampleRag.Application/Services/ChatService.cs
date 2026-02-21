using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using System.Linq.Expressions;

namespace SampleRag.Application.Services;

public class ChatService(
    IDataGenerator dataGenerator,
    IRepository<Guid, Chat> chatRepository) : IChatService
{
    public async Task<IEnumerable<Chat>> GetBatchByAsync(Expression<Func<Chat, bool>> expression, int batchSize)
    {
        return await chatRepository.GetBatchByAsync(expression, batchSize);
    }

    public async Task<IEnumerable<Chat>> GetByIdsAsync(params Guid[] ids)
    {
        return await chatRepository.GetByIdsAsync(ids);
    }

    public async Task RemoveByIdsAsync(params Guid[] ids)
    {
        await chatRepository.RemoveByIdsAsync(ids);
    }

    public async IAsyncEnumerable<MessagePart> StartNewChat(Message firstUserMessage)
    {
        var newChat = new Chat
        {
            Name = "New chat",
        };

        await chatRepository.AddAsync([newChat]);

        firstUserMessage.ChatId = newChat.Id;
        yield return new MessagePart
        {
            NewChatId = newChat.Id
        };

        newChat.Name = string.Empty;
        await foreach (var part in dataGenerator.GenerateStreamingData(firstUserMessage.Text))
        {
            newChat.Name += part;
            yield return new MessagePart
            {
                Text = part
            };
        }

        await chatRepository.UpdateAsync([newChat]);
    }

    public async Task UpdateAsync(params Chat[] items)
    {
        await chatRepository.UpdateAsync(items);
    }
}
