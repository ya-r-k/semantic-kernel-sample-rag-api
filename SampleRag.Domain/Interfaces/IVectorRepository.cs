using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Interfaces;

public interface IVectorRepository<TEntity> where TEntity : IVectorEntity<Guid, float>
{
    Task UpsertChunksAsync(TEntity[] chunks, CancellationToken ct = default);

    Task RemoveByAsync(Guid documentId, CancellationToken ct = default);

    Task<IEnumerable<TEntity>> RetrieveChunksAsync(string query, int topK = 5, CancellationToken ct = default);

    /// <summary>
    /// Retrieves chunks filtered by scope for RAG. Returns chunks with DocumentId and PageNumber for source citation.
    /// </summary>
    Task<IEnumerable<TEntity>> RetrieveChunksAsync(Guid scopeId, string query, int topK = 5, CancellationToken ct = default);
}
