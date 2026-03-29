using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.RequestModels;

public class SendMessageRequest : IEntityWithScopeId
{
    public Guid? ChatId { get; set; }

    public Guid ScopeId { get; set; }

    public string? Text { get; set; }
}
