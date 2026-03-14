namespace SampleRag.Domain.RequestModels;

public class GetDocumentsByModel : GetBatchByModel
{
    public Guid? ScopeId { get; set; }

    public bool? IsChunked { get; set; }
}
