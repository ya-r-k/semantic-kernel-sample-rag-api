namespace SampleRag.Domain.RequestModels;

public class FeedbackRequest
{
    public Guid MessageId { get; set; }

    public bool IsLike { get; set; }
}
