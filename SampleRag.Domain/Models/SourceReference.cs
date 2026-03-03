namespace SampleRag.Domain.Models;

/// <summary>
/// Value type for source citation in RAG responses.
/// </summary>
public class SourceReference
{
    public Guid ChunkId { get; set; }

    public Guid DocumentId { get; set; }

    public int PageNumber { get; set; }
}
