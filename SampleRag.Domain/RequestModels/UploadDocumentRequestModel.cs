namespace SampleRag.Domain.RequestModels;

public class UploadDocumentRequestModel
{
    public string Name { get; set; } = null!;

    public Guid ScopeId { get; set; }

    public FileDataRequestModel File { get; set; } = null!;
}
