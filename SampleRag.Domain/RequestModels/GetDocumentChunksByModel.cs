namespace SampleRag.Domain.RequestModels;

public class GetDocumentChunksByModel : GetBatchByModel
{
    public Guid? DocumentId { get; set; }

    public bool? IsVectorized { get; set; }
}
