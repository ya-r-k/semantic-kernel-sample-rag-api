namespace SampleRag.Domain.RequestModels;

public class UpdateDocumentRequestModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid ScopeId { get; set; }

    public string OriginalLink { get; set; }

    public bool IsChunked { get; set; }
}
