using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Entities;

public class Document : IVectorEntity<Guid, float>, IEntity<Guid>, IEntityWithScopeId
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string LocalLink { get; set; } = null!;

    public string OriginalLink { get; set; } = string.Empty;

    public Guid ScopeId { get; set; }

    public string BriefDescription { get; set; } = string.Empty;

    public bool IsChunked { get; set; }

    public ReadOnlyMemory<float> Vector { get; set; }
}
