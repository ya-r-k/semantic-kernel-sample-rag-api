using MongoDB.Driver;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class KnowledgeScopeUserRepository(IMongoDatabase database) : IKnowledgeScopeUserRepository
{
    private static readonly string CollectionName = "KnowledgeScopesUsers";
    private readonly IMongoCollection<KnowledgeScopeUser> collection = database.GetCollection<KnowledgeScopeUser>(CollectionName);

    public async Task<bool> HasAccessAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScopeUser>.Filter.And(
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.ScopeId, scopeId),
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.UserId, userId));

        return await collection.CountDocumentsAsync(filter, cancellationToken: ct) > 0;
    }

    public async Task<IEnumerable<KnowledgeScopeUser>> AddAsync(KnowledgeScopeUser[] items, CancellationToken ct = default)
    {
        var addedItems = new List<KnowledgeScopeUser>(items);

        try
        {
            await collection.InsertManyAsync(addedItems, new InsertManyOptions { IsOrdered = false }, ct);
        }
        catch (MongoBulkWriteException<KnowledgeScopeUser> ex)
        {
            addedItems = [.. addedItems.Except(ex.WriteErrors.Select(err => items[err.Index]))];
        }

        return addedItems;
    }

    public async Task RemoveUserAsync(Guid scopeId, string[] usersId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScopeUser>.Filter.And(
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.ScopeId, scopeId),
            Builders<KnowledgeScopeUser>.Filter.In(x => x.UserId, usersId));

        await collection.DeleteManyAsync(filter, ct);
    }

    public async Task<IEnumerable<Guid>> GetScopeIdsForUserAsync(GetBatchByModel filterModel, string userId, CancellationToken ct = default)
    {
        var sortDefinition = Builders<KnowledgeScopeUser>.Sort.Ascending("_id");
        var filterBuilder = Builders<KnowledgeScopeUser>.Filter;
        var filter = Builders<KnowledgeScopeUser>.Filter.Empty;

        filter &= filterBuilder.Where(x => x.UserId == userId);

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        var query = collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        var users = await query.ToListAsync();
        return users.Select(u => u.ScopeId).Distinct();
    }
}
