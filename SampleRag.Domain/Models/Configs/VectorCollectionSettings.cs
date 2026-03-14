namespace SampleRag.Domain.Models.Configs;

public class VectorCollectionSettings
{
    public string? CollectionName { get; set; }

    public ulong VectorSize { get; set; }

    public string? Distance { get; set; }

    public string? Quantization { get; set; }
}
