using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Entities;

public class DocumentChunk : IVectorEntity<Guid, float>, IEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public Guid ScopeId { get; set; }

    public int PageNumber { get; set; }

    public int? ChunkIndex { get; set; }

    public string? Text { get; set; }

    public bool IsVectorized { get; set; }

    public string DocumentIdValue
    {
        get => this.DocumentId.ToString();
        set => DocumentId = Guid.Parse(value);
    }

    public string ScopeIdValue
    {
        get => this.ScopeId.ToString();
        set => ScopeId = Guid.Parse(value);
    }

    public ReadOnlyMemory<float> Vector { get; set; }
}
