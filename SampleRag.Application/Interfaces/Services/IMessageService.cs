using SampleRag.Domain.Models;

namespace SampleRag.Application.Interfaces.Services;

public interface IMessageService
{
    IAsyncEnumerable<MessagePart> GenerateAiResponce(MessageData message, int historyWindow = 30);

    Task<IEnumerable<MessageData>> AddAsync(params MessageData[] items);

    Task RemoveByIdsAsync(params int[] ids);

    Task<IEnumerable<MessageData>> GetByIdsAsync(params int[] ids);
}
