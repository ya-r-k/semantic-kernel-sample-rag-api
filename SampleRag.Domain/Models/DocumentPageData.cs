using Microsoft.Extensions.VectorData;

namespace SampleRag.Domain.Models;

public class DocumentPageData
{
    [VectorStoreKey]
    public Guid Key { get; set; } = Guid.NewGuid();

    [VectorStoreData]
    public int PageNumber { get; set; }

    [VectorStoreData]
    public int DocumentId { get; set; }

    [VectorStoreData]
    public string PageContent { get; set; } = null!;

    [VectorStoreVector(768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float>? PageContentEmbedding { get; set; }
}
