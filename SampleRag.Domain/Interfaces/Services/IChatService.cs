using System.Linq.Expressions;
using SampleRag.Domain.Models;

namespace SampleRag.Domain.Interfaces.Services;

public interface IChatService
{
    IAsyncEnumerable<MessagePart> StartNewChat(Message message);

    Task UpdateAsync(params Chat[] items);

    Task RemoveByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Chat>> GetByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Chat>> GetBatchByAsync(Expression<Func<Chat, bool>> expression, int batchSize);
}
