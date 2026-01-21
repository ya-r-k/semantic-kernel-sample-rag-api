namespace SampleRag.Domain.Models;

public class MessageGenerationStatusData
{
    public string[]? Links { get; set; }

    public string Data { get; set; }

    public bool IsFinal { get; set; }

    public DateTime? SavedGeneratedDate { get; set; }
}
