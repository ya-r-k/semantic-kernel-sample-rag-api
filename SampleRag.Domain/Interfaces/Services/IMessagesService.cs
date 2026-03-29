using SampleRag.Domain.Entities;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

public interface IMessagesService
{
    IAsyncEnumerable<MessagePartResponse> GenerateAiResponce(SendMessageRequest message, string role, string userId);

    Task<IEnumerable<Message>> GetBatchByAsync(GetMessagesByModel model);
}
