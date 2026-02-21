using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Models;

/// <summary>
/// Associates a user with a scope. Uniqueness: (ScopeId, UserId).
/// Persistence: MongoDB collection ScopeUsers.
/// </summary>
public class KnowledgeScopeUser : IEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid ScopeId { get; set; }

    public string UserId { get; set; } = null!;
}
