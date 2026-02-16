using Microsoft.Extensions.VectorData;

namespace SampleRag.Infrastructure.VectorStore;

/// <summary>
/// Qdrant payload schema for document chunks. Mapping in Infrastructure per constitution VII (no attributes on domain models).
/// Payload fields: DocumentId, ScopeId, PageNumber, ChunkIndex, Text.
/// </summary>
public class DocumentChunkPayload
{
    [VectorStoreKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [VectorStoreData]
    public Guid DocumentId { get; set; }

    [VectorStoreData]
    public Guid ScopeId { get; set; }

    [VectorStoreData]
    public int PageNumber { get; set; }

    [VectorStoreData]
    public int? ChunkIndex { get; set; }

    [VectorStoreData]
    public string Text { get; set; } = null!;

    [VectorStoreVector(384, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
