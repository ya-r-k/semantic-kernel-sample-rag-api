namespace SampleRag.Domain.Models;

public class DocumentData : Entity<int>
{
    public Guid Key { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = null!;

    public string LocalLink { get; set; } = null!;

    public string OriginalLink { get; set; } = null!;

    public int[] KnowledgeGroupIds { get; set; } = null!;

    public string BriefDescription { get; set; } = null!;
}
