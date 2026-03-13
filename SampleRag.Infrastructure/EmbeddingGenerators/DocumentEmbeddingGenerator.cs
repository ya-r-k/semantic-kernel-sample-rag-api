using Microsoft.Extensions.AI;
using SampleRag.Domain.Entities.Db;

namespace SampleRag.Infrastructure.EmbeddingGenerators;

public class DocumentEmbeddingGenerator(
    IEmbeddingGenerator<string, Embedding<float>> innerGenerator) : IEmbeddingGenerator<Document, Embedding<float>>
{
    private const int MaxBatchSize = 200;

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<Document> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var texts = values.Select(document => document.BriefDescription ?? string.Empty).ToList();
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
