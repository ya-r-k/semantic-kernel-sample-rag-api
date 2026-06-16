using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Entities;

public class Message : IEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public Guid? ScopeId { get; set; }

    public string? Text { get; set; }

    public bool AiGenerated { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets for AI messages: document + page list cited as sources.
    /// </summary>
    public SourceReference[]? SourceReferences { get; set; }

    /// <summary>
    /// Gets or sets the number of prompt (input) tokens used for this message.
    /// </summary>
    public int? PromptTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of completion (output) tokens used for this message.
    /// Only populated for AI-generated messages.
    /// </summary>
    public int? CompletionTokens { get; set; }

    /// <summary>
    /// Gets or sets the total number of tokens used for this message.
    /// Only populated for AI-generated messages.
    /// </summary>
    public int? TotalTokens { get; set; }
    public bool UsesOutdatedSources { get; set; }
}
