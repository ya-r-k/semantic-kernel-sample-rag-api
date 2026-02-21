using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;
using System.Linq.Expressions;

namespace SampleRag.Domain.Interfaces.Services;

public interface IMessagesService
{
    IAsyncEnumerable<MessagePart> GenerateAiResponce(SendMessageRequest message);

    Task<IEnumerable<Message>> GetBatchByAsync(Expression<Func<Message, bool>> expression, int batchSize);   
}
