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
}
