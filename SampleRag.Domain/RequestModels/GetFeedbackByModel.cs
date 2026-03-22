namespace SampleRag.Domain.RequestModels;

public class GetFeedbackByModel : GetBatchByModel
{
    public Guid? MessageId { get; set; }

    public bool? IsLike { get; set; }
}
