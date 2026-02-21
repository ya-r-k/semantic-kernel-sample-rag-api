using MongoDB.Driver;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class KnowledgeScopeUserRepository(IMongoDatabase database) : IKnowledgeGroupUserRepository
{
    private static readonly string CollectionName = "KnowledgeScopesUsers";
    private readonly IMongoCollection<KnowledgeScopeUser> _collection = database.GetCollection<KnowledgeScopeUser>(CollectionName);

    public async Task<bool> HasAccessAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScopeUser>.Filter.And(
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.ScopeId, scopeId),
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.UserId, userId));
        
        return await _collection.CountDocumentsAsync(filter, cancellationToken: ct) > 0;
    }

    public async Task AddUserAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var existing = await HasAccessAsync(scopeId, userId, ct);
        if (existing)
        {
            return;
        }

        var scopeUser = new KnowledgeScopeUser { ScopeId = scopeId, UserId = userId };
        await _collection.InsertOneAsync(scopeUser, cancellationToken: ct);
    }

    public async Task RemoveUserAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScopeUser>.Filter.And(
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.ScopeId, scopeId),
            Builders<KnowledgeScopeUser>.Filter.Eq(x => x.UserId, userId));

        await _collection.DeleteManyAsync(filter, ct);
    }

    public async Task<IEnumerable<Guid>> GetScopeIdsForUserAsync(string userId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScopeUser>.Filter.Eq(x => x.UserId, userId);
        var cursor = await _collection.FindAsync(filter, cancellationToken: ct);
        var users = await cursor.ToListAsync(ct);

        return users.Select(u => u.ScopeId).Distinct();
    }
}
