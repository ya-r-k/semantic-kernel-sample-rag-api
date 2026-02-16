namespace SampleRag.Application.Interfaces.Services;

/// <summary>
/// Chunk text from PDF documents for RAG ingestion.
/// </summary>
public interface IChunkingService
{
    /// <summary>
    /// Extract text chunks from a PDF file. Page-based with optional sub-split for long pages.
    /// </summary>
    /// <param name="pdfPath">Full path to the PDF file.</param>
    /// <param name="documentId">Document ID for chunk metadata.</param>
    /// <param name="scopeId">Scope ID for chunk metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Chunks with DocumentId, ScopeId, PageNumber, ChunkIndex, Text.</returns>
    Task<IReadOnlyList<DocumentChunkInfo>> ChunkPdfAsync(string pdfPath, Guid documentId, Guid scopeId, CancellationToken ct = default);
}

/// <summary>
/// Metadata for a document chunk (before embedding).
/// </summary>
public record DocumentChunkInfo(Guid DocumentId, Guid ScopeId, int PageNumber, int? ChunkIndex, string Text);
