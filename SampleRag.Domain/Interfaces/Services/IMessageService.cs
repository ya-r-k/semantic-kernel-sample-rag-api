using SampleRag.Domain.Models;

namespace SampleRag.Application.Interfaces.Services;

public interface IMessageService<TId> where TId : unmanaged
{
    IAsyncEnumerable<MessagePart> GenerateAiResponce(MessageData message, int historyWindow = 30);

    Task<IEnumerable<MessageData>> AddAsync(params MessageData[] items);

    Task RemoveByIdsAsync(params TId[] ids);

    Task<IEnumerable<MessageData>> GetByIdsAsync(params TId[] ids);
}
