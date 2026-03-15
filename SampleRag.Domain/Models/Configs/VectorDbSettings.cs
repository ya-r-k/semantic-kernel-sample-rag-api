namespace SampleRag.Domain.Models.Configs;

public class VectorDbSettings
{
    public string Url { get; set; } = null!;

    public VectorCollectionSettings[] Collections { get; set; } = null!;

    public ulong TextVectorSize { get; set; }
}
