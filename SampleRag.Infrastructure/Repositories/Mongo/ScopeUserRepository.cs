using MongoDB.Driver;
using SampleRag.Application.Interfaces;
using SampleRag.Domain.Models;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class ScopeUserRepository(IMongoDatabase database) : IScopeUserRepository
{
    private static readonly string CollectionName = "ScopeUsers";
    private readonly IMongoCollection<ScopeUser> _collection = database.GetCollection<ScopeUser>(CollectionName);

    public async Task<bool> HasAccessAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var filter = Builders<ScopeUser>.Filter.And(
            Builders<ScopeUser>.Filter.Eq(x => x.ScopeId, scopeId),
            Builders<ScopeUser>.Filter.Eq(x => x.UserId, userId));
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
        return count > 0;
    }

    public async Task AddUserAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var existing = await HasAccessAsync(scopeId, userId, ct);
        if (existing)
            return;

        var scopeUser = new ScopeUser { ScopeId = scopeId, UserId = userId };
        await _collection.InsertOneAsync(scopeUser, cancellationToken: ct);
    }

    public async Task RemoveUserAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        var filter = Builders<ScopeUser>.Filter.And(
            Builders<ScopeUser>.Filter.Eq(x => x.ScopeId, scopeId),
            Builders<ScopeUser>.Filter.Eq(x => x.UserId, userId));
        await _collection.DeleteManyAsync(filter, ct);
    }

    public async Task<IEnumerable<Guid>> GetScopeIdsForUserAsync(string userId, CancellationToken ct = default)
    {
        var filter = Builders<ScopeUser>.Filter.Eq(x => x.UserId, userId);
        var cursor = await _collection.FindAsync(filter, cancellationToken: ct);
        var users = await cursor.ToListAsync(ct);
        return users.Select(u => u.ScopeId).Distinct();
    }
}
