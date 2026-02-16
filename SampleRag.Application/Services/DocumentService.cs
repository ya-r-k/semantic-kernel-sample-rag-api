using SampleRag.Application.Interfaces;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Configs;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class DocumentService(
    IRepository<Guid, DocumentData> documentsRepository,
    IFileRepository fileRepository,
    IChunkingService chunkingService,
    IDocumentChunkStore chunkStore,
    FilesStorageSettings storageSettings) : IDocumentService
{
    public async Task<IEnumerable<DocumentData>> AddAsync(params UploadDocumentRequestModel[] items)
    {
        var savingData = items.Select(x => new DocumentData
        {
            Name = x.Name,
            ScopeId = x.ScopeId,
        }).ToArray();

        for (var i = 0; i < savingData.Length; i++)
        {
            savingData[i].LocalLink = await fileRepository.SaveAsync(items[i].File.Data, items[i].File.FileName);
        }

        var result = (await documentsRepository.AddAsync(savingData)).ToList();

        for (var i = 0; i < result.Count; i++)
        {
            var doc = result[i];
            var fullPath = Path.Combine(storageSettings.BasePath ?? "", "assets/documents", doc.LocalLink);
            if (File.Exists(fullPath))
            {
                var chunks = await chunkingService.ChunkPdfAsync(fullPath, doc.Id, doc.ScopeId);
                await chunkStore.UpsertChunksAsync(chunks);
            }
        }

        return result;
    }

    public async Task<IEnumerable<DocumentData>> GetByIdsAsync(params Guid[] ids)
    {
        return await documentsRepository.GetByIdsAsync(ids);
    }

    public Task RemoveByIdsAsync(params Guid[] ids)
    {
        return documentsRepository.RemoveByIdsAsync(ids);
    }
}
