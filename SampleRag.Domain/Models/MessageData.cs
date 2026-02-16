using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Models;

public class MessageData : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string Text { get; set; }

    public bool AiGenerated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid ChatId { get; set; }

    public int[]? DocumentPagesIds { get; set; }
}
