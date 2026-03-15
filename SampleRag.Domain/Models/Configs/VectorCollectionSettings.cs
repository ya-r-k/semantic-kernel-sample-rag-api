namespace SampleRag.Domain.Models.Configs;

public class VectorCollectionSettings
{
    public string CollectionName { get; set; } = null!;

    public ulong VectorSize { get; set; }

    public string Distance { get; set; } = null!;

    public string Quantization { get; set; } = null!;
}
