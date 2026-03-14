using Mapster;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class DocumentService(
    IDocumentChunkService documentChunkService,
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

    public async Task RemoveAllChunksAsync(CancellationToken ct = default)
    {
        await documentsRepository.SetFieldValueAsync(x => x.IsChunked, false);
        await documentChunkService.RemoveAllAsync(ct);
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
