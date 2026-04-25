using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class ChatService(IFilterRepository<Guid, Chat, GetChatsByModel> repository) : IChatService
{
    public async Task<IEnumerable<Chat>> AddAsync(params Chat[] items)
    {
        return await repository.AddAsync(items);
    }

    public async Task<IEnumerable<Chat>> GetBatchByAsync(GetChatsByModel model)
    {
        return await repository.GetBatchByAsync(model);
    }

    public async Task<IEnumerable<Chat>> GetByIdsAsync(params Guid[] ids)
    {
        return await repository.GetByIdsAsync(ids);
    }

    public async Task RemoveByIdsAsync(params Guid[] ids)
    {
        await repository.RemoveByIdsAsync(ids);
    }

    public async Task UpdateAsync(params Chat[] items)
    {
        await repository.UpdateAsync(items);
    }
    public async Task<bool> HasAccessAsync(Guid chatId, string userId, CancellationToken ct = default)
    {
        var chats = await repository.GetByIdsAsync(new[] { chatId }, ct);
        var chat = chats.FirstOrDefault();
        if (chat == null)
            return false;
        if (chat.OwnerId == userId)
            return true;
        if (chat.UsersIds != null && chat.UsersIds.Contains(userId))
            return true;
        return false;
    }
}
