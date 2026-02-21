using SampleRag.Domain.Models;
using System.Linq.Expressions;

namespace SampleRag.Domain.Interfaces.Services;

/// <summary>
/// Chunk text from PDF documents for RAG ingestion.
/// </summary>
public interface IDocumentChunkService
{
    Task<IEnumerable<DocumentChunk>> ChunkAsync(Document data, CancellationToken ct = default);

    Task<IEnumerable<DocumentChunk>> GetBatchByAsync(Expression<Func<DocumentChunk, bool>> expression, int batchSize);
}
