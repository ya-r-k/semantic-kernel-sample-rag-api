using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Entities;

public class Chat : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets chat bound to one scope; RAG uses only this scope's documents.
    /// </summary>
    public Guid ScopeId { get; set; }

    /// <summary>
    /// Gets or sets user identifiers who can send/receive and add owners (from token sub).
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    /// Gets or sets legacy: kept for backward compatibility during migration.
    /// Prefer OwnerIds for new chats.
    /// </summary>
    [Obsolete("Use OwnerIds instead")]
    public int[]? UsersIds { get; set; }
}
