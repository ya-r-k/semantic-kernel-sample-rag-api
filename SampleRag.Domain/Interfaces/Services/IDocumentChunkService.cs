using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

/// <summary>
/// Chunk text from PDF documents for RAG ingestion.
/// </summary>
public interface IDocumentChunkService
{
    Task<IEnumerable<DocumentChunk>> ChunkAsync(Document data, CancellationToken ct = default);

    Task<IEnumerable<DocumentChunk>> GetBatchByAsync(GetDocumentChunksByModel model);
    
    Task<IEnumerable<DocumentChunk>> RetrieveChunksAsync(string query, int topK = 5, CancellationToken ct = default);

    Task RemoveAllAsync(CancellationToken ct = default);

    Task RemoveAllEmbeddingsAsync(CancellationToken ct = default);
}
