using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.Infrastructure.Repositories.Vector;

public class QdrantDocumentChunkRepository(
    IEmbeddingGenerator<DocumentChunk, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    VectorDbSettings settings) : IVectorRepository<DocumentChunk>
{
    private readonly VectorStoreCollection<Guid, DocumentChunk> vectorCollection = 
        vectorStore.GetCollection<Guid, DocumentChunk>("document-chunks", new VectorStoreCollectionDefinition
        {
            EmbeddingGenerator = embeddingGenerator,
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(Guid)),
                new VectorStoreDataProperty("PageNumber", typeof(int)),
                new VectorStoreDataProperty("ChunkIndex", typeof(int?)),
                new VectorStoreVectorProperty("Vector", typeof(ReadOnlyMemory<float>), dimensions: 1024) 
                { 
                    DistanceFunction = DistanceFunction.CosineSimilarity,
                    IndexKind = IndexKind.Hnsw,
                },
            ]
        });

    public async Task UpsertChunksAsync(DocumentChunk[] chunks, CancellationToken ct = default)
    {
        if (chunks.Length == 0)
        {
            return;
        }

        var embeddings = await embeddingGenerator.GenerateAsync(chunks, cancellationToken: ct);
        for (var i = 0; i < chunks.Length; i++)
        {
            chunks[i].Vector = embeddings[i].Vector;
        }

        await vectorCollection.UpsertAsync(chunks, ct);
    }

    public async Task<IEnumerable<DocumentChunk>> RetrieveChunksAsync(string query, int topK = 5, CancellationToken ct = default)
    {
        var queryEmbedding = await embeddingGenerator.GenerateAsync(
        [
            new DocumentChunk
            {
                Text = query,
            }
        ], cancellationToken: ct);

        var result = await vectorCollection.SearchAsync(new DocumentChunk
        {
            Text = query,
            Vector = queryEmbedding[0].Vector,
        }, topK, cancellationToken: ct).ToListAsync(cancellationToken: ct);

        return [.. result.Select(x => x.Record)];
    }

    public async Task RemoveByAsync(Guid documentId, CancellationToken ct = default)
    {
        using var qdrantClient = new QdrantClient(settings.Url);

        var filter = new Filter();
        filter.Must.Add(new Condition()
        {
            Field = new FieldCondition
            {
                Key = nameof(DocumentChunk.DocumentId),
                Match = new Match()
                {
                    Keyword = documentId.ToString(),
                }
            },
        });

        await qdrantClient.DeleteAsync("document-chunks", filter, cancellationToken: ct);
    }
}
