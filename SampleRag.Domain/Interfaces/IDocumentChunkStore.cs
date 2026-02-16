using SampleRag.Application.Interfaces.Services;

namespace SampleRag.Application.Interfaces;

/// <summary>
/// Stores document chunks with embeddings in the vector store.
/// </summary>
public interface IDocumentChunkStore
{
    Task UpsertChunksAsync(IReadOnlyList<DocumentChunkInfo> chunks, CancellationToken ct = default);

    Task RemoveChunksByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
}
