namespace SampleRag.Domain.Models.Configs;

public class VectorDbSettings
{
    public string? Url { get; set; }

    public VectorCollectionSettings[]? Collections { get; set; }

    public ulong TextVectorSize { get; set; }
}
