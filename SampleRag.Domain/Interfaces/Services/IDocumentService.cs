using SampleRag.Domain.Entities;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

public interface IDocumentService
{
    Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] request);

    Task UpdateAsync(Document[] items, string[] fields);

    Task RemoveAllChunksAsync(CancellationToken ct = default);

    Task RemoveByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Document>> GetByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Document>> GetBatchByAsync(GetDocumentsByModel model);
}
