using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<IEnumerable<DocumentData>> AddAsync(params UploadDocumentRequestModel[] items);

    Task RemoveByIdsAsync(params int[] ids);

    Task<IEnumerable<DocumentData>> GetByIdsAsync(params int[] ids);
}
