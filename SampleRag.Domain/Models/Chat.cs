using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Models;

public class Chat : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Chat bound to one scope; RAG uses only this scope's documents.
    /// </summary>
    public Guid ScopeId { get; set; }

    /// <summary>
    /// User identifiers who can send/receive and add owners (from token sub).
    /// </summary>
    public string[] OwnerIds { get; set; } = [];

    /// <summary>
    /// Legacy: kept for backward compatibility during migration.
    /// Prefer OwnerIds for new chats.
    /// </summary>
    [Obsolete("Use OwnerIds instead")]
    public int[]? UsersIds { get; set; }
}
