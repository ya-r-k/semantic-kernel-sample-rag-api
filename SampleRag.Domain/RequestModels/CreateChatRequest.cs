using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.RequestModels;

public class CreateChatRequest : IEntityWithScopeId
{
    public string Name { get; set; } = string.Empty;

    public Guid ScopeId { get; set; }

    public string[] UsersIds { get; set; } = null!;
}
