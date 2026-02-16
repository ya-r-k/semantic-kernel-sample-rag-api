using Qdrant.Client;
using Qdrant.Client.Grpc;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.Infrastructure.VectorStore;

/// <summary>
/// Ensures Qdrant collection exists with schema for DocumentChunkPayload.
/// Payload: DocumentId, ScopeId, PageNumber, ChunkIndex, Text.
/// </summary>
public static class QdrantCollectionConfig
{
    public const string CollectionName = "document-chunks";

    public static async Task EnsureCollectionExistsAsync(
        QdrantClient client,
        VectorDbSettings settings,
        CancellationToken ct = default)
    {
        var vectorSize = (ulong)(settings.TextVectorSize > 0 ? settings.TextVectorSize : 384);
        var collections = await client.ListCollectionsAsync(ct);
        var exists = collections.Any(c => string.Equals(c, CollectionName, StringComparison.OrdinalIgnoreCase));
        if (exists)
            return;

        await client.CreateCollectionAsync(CollectionName, new VectorParams { Size = vectorSize, Distance = Distance.Cosine });
    }
}
