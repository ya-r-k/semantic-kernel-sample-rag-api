using System.Text.Json.Serialization;

namespace SampleRag.Domain.Models;

public class MessagePart 
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Text { get; set; }

    public DateTime? CreatedAt { get; set; }
}
