namespace SampleRag.Domain.Models;

public class MessageData
{
    public int? Id { get; set; }

    public string Text { get; set; }

    public bool AiGenerated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ConversationId { get; set; }

    public int[]? DocumentPagesIds { get; set; }
}
