using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Models;

public class DocumentChunk : IVectorEntity<Guid, float>, IEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public Guid ScopeId { get; set; }

    public int PageNumber { get; set; }

    public int? ChunkIndex { get; set; }

    public string Text { get; set; }

    public bool IsVectorized { get; set; }

    public ReadOnlyMemory<float> Vector { get; set; }
}
