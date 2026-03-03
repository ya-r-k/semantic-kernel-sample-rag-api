using System.Linq.Expressions;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

public interface IMessagesService
{
    IAsyncEnumerable<MessagePart> GenerateAiResponce(SendMessageRequest message, string userId);

    Task<IEnumerable<Message>> GetBatchByAsync(Expression<Func<Message, bool>> expression, int batchSize);
}
