using Microsoft.Extensions.AI;
using SampleRag.Domain.Entities;

namespace SampleRag.Infrastructure.EmbeddingGenerators;

public class DocumentChunkEmbeddingGenerator(
    IEmbeddingGenerator<string, Embedding<float>> innerGenerator) : IEmbeddingGenerator<DocumentChunk, Embedding<float>>
{
    private const int MaxBatchSize = 50;

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<DocumentChunk> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var texts = values.Select(chunk => chunk.Text ?? string.Empty).ToList();
        var batches = texts.Chunk(MaxBatchSize);

        var generatedEmbeddings = new List<Embedding<float>>();
        foreach (var batch in batches)
        {
            generatedEmbeddings.AddRange(await innerGenerator.GenerateAsync(
                batch,
                options,
                cancellationToken));
        }

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
