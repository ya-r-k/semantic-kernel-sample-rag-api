using System.Linq.Expressions;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

public interface IDocumentService
{
    Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] items);

    Task UpdateAsync(params Document[] items);

    Task RemoveAllChunksAsync(CancellationToken ct = default);

    Task RemoveByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Document>> GetByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Document>> GetBatchByAsync(GetDocumentsByModel model);
}
