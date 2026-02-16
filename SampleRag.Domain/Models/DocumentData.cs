using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Models;

public class DocumentData : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string LocalLink { get; set; } = null!;

    public string OriginalLink { get; set; } = string.Empty;

    public Guid ScopeId { get; set; }

    public string BriefDescription { get; set; } = string.Empty;
}
