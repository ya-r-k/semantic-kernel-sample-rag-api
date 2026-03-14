using SampleRag.Domain.Entities;
using SampleRag.Domain.Models;

namespace SampleRag.Domain.Interfaces;

public interface IDataGenerator
{
    IAsyncEnumerable<string> GenerateStreamingData(string message, CancellationToken ct = default);

    IAsyncEnumerable<string> GenerateStreamingData(string message, string executionSettingsName, CancellationToken ct = default);

    IAsyncEnumerable<MessagePartResponse> GenerateStreamingData(IEnumerable<Message> messages, CancellationToken ct = default);

    IAsyncEnumerable<MessagePartResponse> GenerateStreamingData(IEnumerable<Message> messages, string executionSettingsName, CancellationToken ct = default);
}
