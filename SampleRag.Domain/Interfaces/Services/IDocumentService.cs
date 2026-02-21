using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;
using System.Linq.Expressions;

namespace SampleRag.Domain.Interfaces.Services;

public interface IDocumentService
{
    Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] items);

    Task UpdateAsync(params Document[] items);

    Task RemoveByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Document>> GetByIdsAsync(params Guid[] ids);

    Task<IEnumerable<Document>> GetBatchByAsync(Expression<Func<Document, bool>> expression, int batchSize);
}
