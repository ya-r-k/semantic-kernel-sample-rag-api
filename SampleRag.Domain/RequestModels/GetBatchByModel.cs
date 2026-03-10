namespace SampleRag.Domain.RequestModels;

public class GetBatchByModel
{
    public Guid? LastId { get; set; }

    public int BatchSize { get; set; }
}
