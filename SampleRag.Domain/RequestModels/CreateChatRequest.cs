using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.RequestModels;

public class CreateChatRequest : IEntityWithScopeId
{
    public string Title { get; set; } = string.Empty;

    public Guid ScopeId { get; set; }

    public string[]? OwnerIds { get; set; }
}
