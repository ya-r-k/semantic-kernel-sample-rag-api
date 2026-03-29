namespace SampleRag.Domain.RequestModels;

public class SendMessageRequest
{
    public Guid? ChatId { get; set; }

    public Guid ScopeId { get; set; }

    public string? Text { get; set; }
}
