namespace SampleRag.Domain.RequestModels;

public class GetChatsByModel : GetBatchByModel
{
    public Guid? ScopeId { get; set; }
}
