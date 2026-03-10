using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

public interface IChatService
{
    Task<IEnumerable<Chat>> AddAsync(params Chat[] items);

    Task UpdateAsync(params Chat[] items);

    Task RemoveByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Chat>> GetByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Chat>> GetBatchByAsync(GetChatsByModel model);
}
