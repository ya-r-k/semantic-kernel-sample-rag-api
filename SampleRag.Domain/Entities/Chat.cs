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
    /// Gets or sets user identifier who can send/receive and add users (from token sub).
    /// </summary>
    public string OwnerId { get; set; }

    public string[] UsersIds { get; set; } = null!;
}
