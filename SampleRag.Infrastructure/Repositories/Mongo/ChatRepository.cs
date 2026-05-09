using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class ChatRepository(IMongoDatabase database): MongoBaseRepository<Chat>(database),IChatRepository
{
    public async Task<IEnumerable<Chat>> GetBatchByAsync(GetChatsByModel filterModel)
    {
        var sortDefinition = Builders<Chat>.Sort.Ascending("_id");
        var filterBuilder = Builders<Chat>.Filter;
        var filter = Builders<Chat>.Filter.Empty;

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        if (filterModel.ScopeId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ScopeId == filterModel.ScopeId.Value);
        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Chat>> GetBatchByAsync(GetChatsByModel filterModel, string userId, CancellationToken ct = default)
    {
        var sortDefinition = Builders<Chat>.Sort.Ascending("_id");
        var filterBuilder = Builders<Chat>.Filter;
        var filter = Builders<Chat>.Filter.Empty;

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        if (filterModel.ScopeId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ScopeId == filterModel.ScopeId.Value);
        }

        filter &= (
            filterBuilder.Eq(x => x.OwnerId, userId)
            | filterBuilder.AnyEq(x => x.UsersIds, userId)
        );

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync(ct);
    }

    public async Task<bool> HasAccessAsync(Guid chatId, string userId, CancellationToken ct = default)
    {
        var filterBuilder = Builders<Chat>.Filter;

        var filter =
            filterBuilder.Eq(x => x.Id, chatId)
            & (
                filterBuilder.Eq(x => x.OwnerId, userId)
                | filterBuilder.AnyEq(x => x.UsersIds, userId)
            );

        return await this.collection.Find(filter).AnyAsync(ct);
    }
}
