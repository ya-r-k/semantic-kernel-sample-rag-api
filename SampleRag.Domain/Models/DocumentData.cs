using Microsoft.Extensions.VectorData;

namespace SampleRag.Domain.Models;

public class DocumentData
{
    [VectorStoreKey]
    public Guid Key { get; set; } = Guid.NewGuid();

    [VectorStoreData]
    public string Name { get; set; } = null!;

    [VectorStoreData]
    public string LocalLink { get; set; } = null!;

    [VectorStoreData]
    public string OriginalLink { get; set; } = null!;

    [VectorStoreData]
    public string UsageGroup { get; set; } = null!;

    [VectorStoreData]
    public string BriefDescription { get; set; } = null!;

    [VectorStoreVector(768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float>? BriefDescriptionEmbedding { get; set; }
}
