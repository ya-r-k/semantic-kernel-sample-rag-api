using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<IEnumerable<DocumentData>> AddAsync(params UploadDocumentRequestModel[] items);

    Task RemoveByIdsAsync(params Guid[] ids);

    Task<IEnumerable<DocumentData>> GetByIdsAsync(params Guid[] ids);
}
