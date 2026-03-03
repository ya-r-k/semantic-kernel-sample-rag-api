using MongoDB.Driver;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;
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

    public async Task AddUserAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var existing = await HasAccessAsync(scopeId, userId, ct);
        if (existing)
        {
            return;
        }

        var scopeUser = new KnowledgeScopeUser { ScopeId = scopeId, UserId = userId };
        await collection.InsertOneAsync(scopeUser, cancellationToken: ct);
    }

    public async Task RemoveUserAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScopeUser>.Filter.And(
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.ScopeId, scopeId),
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.UserId, userId));

        await collection.DeleteManyAsync(filter, ct);
    }

    public async Task<IEnumerable<Guid>> GetScopeIdsForUserAsync(GetBatchByModel filterModel, string userId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScopeUser>.Filter.Eq(x => x.UserId, userId);
        var cursor = await collection.FindAsync(filter, cancellationToken: ct);
        var users = await cursor.ToListAsync(ct);

        return users.Select(u => u.ScopeId).Distinct();
    }
}
