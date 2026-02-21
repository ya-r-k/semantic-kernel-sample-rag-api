using Microsoft.Extensions.AI;
using SampleRag.Domain.Models;

namespace SampleRag.Infrastructure.EmbeddingGenerators;

public class DocumentEmbeddingGenerator(
    IEmbeddingGenerator<string, Embedding<float>> innerGenerator) : IEmbeddingGenerator<Document, Embedding<float>>
{
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<Document> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var texts = values.Select(chunk => chunk.BriefDescription ?? string.Empty).ToList();

        var generatedEmbeddings = await innerGenerator.GenerateAsync(
            texts,
            options,
            cancellationToken);

        return new GeneratedEmbeddings<Embedding<float>>([.. generatedEmbeddings.Select(e => e)]);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return innerGenerator.GetService(serviceType, serviceKey);
    }

    public void Dispose()
    {
        if (innerGenerator is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
