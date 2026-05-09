using SampleRag.Domain.Entities;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces;

public interface IChatRepository : IFilterRepository<Guid, Chat, GetChatsByModel>
{
    Task<IEnumerable<Chat>> GetBatchByAsync(GetChatsByModel filterModel, string userId, CancellationToken ct = default);

    Task<bool> HasAccessAsync(Guid chatId, string userId, CancellationToken ct = default);
}
