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
    public async Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] request)
    {
        var savingData = request.Adapt<Document[]>();
        for (var i = 0; i < request.Length; i++)
        {
            savingData[i].LocalLink = await fileRepository.SaveAsync(
                Path.Combine("assets", "documents", request[i].ScopeId.ToString()),
                request[i].File.FileName,
                request[i].File.Content);
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

    public async Task UpdateAsync(Document[] items, string[] fields)
    {
        var itemIds = items.Select(x => x.Id).ToArray();
        var existingDocs = await this.GetByIdsAsync(itemIds);
        var existingById = existingDocs.ToDictionary(
            doc => doc.Id.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var transformedItems = new List<Dictionary<string, object?>>();

        foreach (var item in items)
        {
            var itemDict = item.Adapt<Dictionary<string, object?>>()
                .Where(kvp => fields.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (existingById.TryGetValue(item.Id.ToString(), out var existingDoc))
            {
                var newFilePath = Path.Combine(
                    "assets",
                    "documents",
                    item.ScopeId.ToString(),
                    Path.GetFileName(existingDoc.LocalLink));
                itemDict[nameof(Document.LocalLink)] = newFilePath;

                await fileRepository.MoveAsync(existingDoc.LocalLink, newFilePath);
            }

            transformedItems.Add(itemDict);
        }

        await documentsRepository.PartialUpdateAsync([.. transformedItems]);
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
}
