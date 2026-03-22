namespace SampleRag.Domain.Models.Configs;

public class GenAiProviderSettings
{
    public string Url { get; set; } = null!;

    public string TextModel { get; set; } = null!;

    public string TextEmbeddingModel { get; set; } = null!;
}
