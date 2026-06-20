using SampleRag.Domain.Entities;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces;

public interface IMessageRepository : IFilterRepository<Guid, Message, GetMessagesByModel>
{
    Task<IEnumerable<Message>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);

    Task<IEnumerable<Message>> GetByChatIdAsync(Guid chatId, CancellationToken ct = default);
}
