using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Entities;

public class Message : IEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public string? Text { get; set; }

    public bool AiGenerated { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets for AI messages: document + page list cited as sources.
    /// </summary>
    public SourceReference[]? SourceReferences { get; set; }
}
