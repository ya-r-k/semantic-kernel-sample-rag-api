using Mapster;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class DocumentService(
    IFilterRepository<Guid, Document, GetDocumentsByModel> documentsRepository,
    IFileRepository fileRepository) : IDocumentService
{
    public async Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] items)
    {
        var savingData = items.Adapt<Document[]>();

        for (var i = 0; i < savingData.Length; i++)
        {
            savingData[i].LocalLink = await fileRepository.SaveAsync("assets\\documents", items[i].File.FileName, items[i].File.Content);
        }

        return await documentsRepository.AddAsync(savingData);
    }

    public async Task<IEnumerable<Document>> GetBatchByAsync(GetDocumentsByModel model)
    {
        return await documentsRepository.GetBatchByAsync(model);
    }

    public async Task<IEnumerable<Document>> GetByIdsAsync(params Guid[] ids)
    {
        return await documentsRepository.GetByIdsAsync(ids);
    }

    public Task RemoveByIdsAsync(params Guid[] ids)
    {
        return documentsRepository.RemoveByIdsAsync(ids);
    }

    public Task UpdateAsync(params Document[] items)
    {
        return documentsRepository.UpdateAsync(items);
    }
}
