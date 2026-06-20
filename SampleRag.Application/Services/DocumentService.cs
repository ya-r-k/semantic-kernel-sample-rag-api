using Mapster;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Repositories;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class DocumentService(
    IDocumentChunkService documentChunkService,
    IDocumentRepository documentsRepository,
    IMessageRepository messageRepository,
    IChatService chatService,
    IKnowledgeScopeRepository scopeRepository,
    IFileRepository fileRepository,
    IVectorRepository<DocumentChunk> vectorRepository) : IDocumentService
{
    public async Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] request)
    {
        var savingData = request.Adapt<Document[]>();
        var scopesIds = savingData.Select(x => x.ScopeId).Distinct().ToArray();
        for (var i = 0; i < request.Length; i++)
        {
            savingData[i].LocalLink = await fileRepository.SaveAsync(
                Path.Combine("assets", "documents", request[i].ScopeId.ToString()),
                request[i].File.FileName,
                request[i].File.Content);
        }

        savingData = [.. await documentsRepository.AddAsync(savingData)];
        if (scopesIds is not null && scopesIds.Length > 0)
        {
            await scopeRepository.RecalculateDocumentsCountAsync(scopesIds);
        }

        return savingData;
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
        var existingDocs = await GetByIdsAsync(itemIds);
        var existingById = existingDocs.ToDictionary(
            doc => doc.Id.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var transformedItems = new List<Dictionary<string, object?>>();

        foreach (var item in items)
        {
            var itemDict = item.Adapt<Dictionary<string, object?>>()
                .Where(kvp => kvp.Key == nameof(Document.Id) || fields.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (!itemDict.ContainsKey(nameof(Document.Id)))
            {
                itemDict[nameof(Document.Id)] = item.Id;
            }

            if (existingById.TryGetValue(item.Id.ToString(), out var existingDoc))
            {
                if (fields.Contains(nameof(Document.ScopeId)) && item.ScopeId != existingDoc.ScopeId)
                {
                    var newFilePath = Path.Combine(
                        "assets",
                        "documents",
                        item.ScopeId.ToString(),
                        Path.GetFileName(existingDoc.LocalLink));

                    itemDict[nameof(Document.LocalLink)] = newFilePath;
                    await fileRepository.MoveAsync(existingDoc.LocalLink, newFilePath);
                }
            }

            transformedItems.Add(itemDict);
        }

        await documentsRepository.PartialUpdateAsync([.. transformedItems]);

        if (fields.Contains(nameof(Document.IsOutOfDate)))
        {
            var outOfDateDocs = items.Where(d => d.IsOutOfDate).ToArray();
            foreach (var doc in outOfDateDocs)
            {
                await vectorRepository.RemoveByAsync(doc.Id);
            }
            await RefreshChatsOutdatedStateAsync(items);
        }
    }

    private async Task RefreshChatsOutdatedStateAsync(Document[] items)
    {
        var updatedDocs = await GetByIdsAsync(items.Select(x => x.Id).ToArray());
        var chatsToUpdate = new List<Chat>();

        foreach (var updatedDoc in updatedDocs)
        {
            var referencedMessages = await messageRepository.GetByDocumentIdAsync(updatedDoc.Id);
            var chatIds = referencedMessages.Select(x => x.ChatId).Distinct().ToArray();
            if (!chatIds.Any())
            {
                continue;
            }

            var chats = (await chatService.GetByIdsAsync(chatIds)).ToArray();
            foreach (var chat in chats)
            {
                if (updatedDoc.IsOutOfDate)
                {
                    if (!chat.HasOutdatedSources)
                    {
                        chat.HasOutdatedSources = true;
                        chatsToUpdate.Add(chat);
                    }

                    continue;
                }

                var chatMessages = await messageRepository.GetByChatIdAsync(chat.Id);
                var referencedDocIds = chatMessages
                    .Where(m => m.SourceReferences != null)
                    .SelectMany(m => m.SourceReferences!)
                    .Select(sr => sr.DocumentId)
                    .Distinct()
                    .ToArray();

                if (!referencedDocIds.Any())
                {
                    if (chat.HasOutdatedSources)
                    {
                        chat.HasOutdatedSources = false;
                        chatsToUpdate.Add(chat);
                    }

                    continue;
                }

                var currentDocs = await GetByIdsAsync(referencedDocIds);
                var stillHasOutdated = currentDocs.Any(d => d.IsOutOfDate);
                if (chat.HasOutdatedSources != stillHasOutdated)
                {
                    chat.HasOutdatedSources = stillHasOutdated;
                    chatsToUpdate.Add(chat);
                }
            }
        }

        if (chatsToUpdate.Any())
        {
            await chatService.UpdateAsync(chatsToUpdate.ToArray());
        }
    }

    public async Task RemoveAllChunksAsync(CancellationToken ct = default)
    {
        await documentsRepository.SetFieldValueAsync(x => x.IsChunked, false);
        await documentChunkService.RemoveAllAsync(ct);
    }

    public async Task RemoveByIdsAsync(params Guid[] ids)
    {
        var documents = await documentsRepository.GetByIdsAsync(ids);
        var scopesIds = documents.Select(x => x.ScopeId)
            .Distinct()
            .ToArray();

        if (scopesIds.Length > 0)
        {
            await scopeRepository.RecalculateDocumentsCountAsync(scopesIds);
        }

        await documentsRepository.RemoveByIdsAsync(ids);
    }
}
