using SampleRag.Domain.Models;

namespace SampleRag.Application.Interfaces.Services;

public interface IDataGenerator
{
    IAsyncEnumerable<string> GenerateStreamingData(string message, CancellationToken ct = default);

    IAsyncEnumerable<string> GenerateStreamingData(IEnumerable<MessageData> messages, CancellationToken ct = default);
}
