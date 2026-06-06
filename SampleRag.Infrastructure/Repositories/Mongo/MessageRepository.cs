using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class MessageRepository(IMongoDatabase database) : MongoBaseRepository<Message>(database), IMessageRepository
{
    public async Task<IEnumerable<Message>> GetBatchByAsync(GetMessagesByModel filterModel)
    {
        var sortDefinition = Builders<Message>.Sort.Ascending(x => x.CreatedAt);
        var filterBuilder = Builders<Message>.Filter;
        var filter = Builders<Message>.Filter.Empty;

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        if (filterModel.ChatId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ChatId == filterModel.ChatId.Value);
        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        var filter = Builders<Message>.Filter.ElemMatch(x => x.SourceReferences, Builders<SourceReference>.Filter.Eq(x => x.DocumentId, documentId));
        return await this.collection.Find(filter).ToListAsync(ct);
    }

    public async Task<IEnumerable<Message>> GetByChatIdAsync(Guid chatId, CancellationToken ct = default)
    {
        var filter = Builders<Message>.Filter.Eq(x => x.ChatId, chatId);
        return await this.collection.Find(filter).ToListAsync(ct);
    }
}
