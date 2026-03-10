namespace SampleRag.Domain.RequestModels;

public class GetMessagesByModel : GetBatchByModel
{
    public Guid? ChatId { get; set; }
}
