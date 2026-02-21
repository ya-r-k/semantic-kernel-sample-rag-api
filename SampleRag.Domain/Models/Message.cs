using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Models;

public class Message : IEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public string Text { get; set; }

    public bool AiGenerated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int[]? DocumentPagesIds { get; set; }
}
