using Mapster;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentChunkService documentChunkService;
    private readonly IFilterRepository<Guid, Document, GetDocumentsByModel> documentsRepository;
    private readonly IMessageRepository messageRepository;
    private readonly IChatService chatService;
    private readonly IFileRepository fileRepository;

    public DocumentService(
        IDocumentChunkService documentChunkService,
        IFilterRepository<Guid, Document, GetDocumentsByModel> documentsRepository,
        IMessageRepository messageRepository,
        IChatService chatService,
        IFileRepository fileRepository)
    {
        this.documentChunkService = documentChunkService;
        this.documentsRepository = documentsRepository;
        this.messageRepository = messageRepository;
        this.chatService = chatService;
        this.fileRepository = fileRepository;
    }

    public async Task<Document?> AddAsync(UploadDocumentRequestModel request)
    {
        var savingData = request.Adapt<Document>();

        savingData.LocalLink = await fileRepository.SaveAsync(
            Path.Combine("assets", "documents", request.ScopeId.ToString()),
            request.File.FileName,
            request.File.Content);

        return (await documentsRepository.AddAsync([savingData])).FirstOrDefault();
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
            await RefreshChatsOutdatedStateAsync(items);
        }
    }

    private async Task RefreshChatsOutdatedStateAsync(Document[] items)
    {
        var updatedDocs = await this.GetByIdsAsync(items.Select(x => x.Id).ToArray());
        var chatsToUpdate = new List<Chat>();

        foreach (var updatedDoc in updatedDocs)
        {
            var referencedMessages = await this.messageRepository.GetByDocumentIdAsync(updatedDoc.Id);
            var chatIds = referencedMessages.Select(x => x.ChatId).Distinct().ToArray();
            if (!chatIds.Any())
            {
                continue;
            }

            var chats = (await this.chatService.GetByIdsAsync(chatIds)).ToArray();
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

                var chatMessages = await this.messageRepository.GetByChatIdAsync(chat.Id);
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

                var currentDocs = await this.GetByIdsAsync(referencedDocIds);
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
            await this.chatService.UpdateAsync(chatsToUpdate.ToArray());
        }
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
