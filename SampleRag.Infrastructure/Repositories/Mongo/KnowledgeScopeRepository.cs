using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Enums;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class KnowledgeScopeRepository(IMongoDatabase database) : MongoBaseRepository<KnowledgeScope>(database), IKnowledgeScopeRepository
{
    public async Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel)
    {
        var sortDefinition = Builders<KnowledgeScope>.Sort.Ascending("_id");
        var builder = Builders<KnowledgeScope>.Filter;
        var filter = Builders<KnowledgeScope>.Filter.Empty;

        if (filterModel.LastId.HasValue)
        {
            filter &= builder.Where(x => x.Id > filterModel.LastId.Value);
        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync();
    }

    public async Task<bool> HasAccessAsync(Guid scopeId, UserRole role, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScope>.Filter.And(
            Builders<KnowledgeScope>.Filter.Eq(x => x.Id, scopeId),
            Builders<KnowledgeScope>.Filter.AnyEq(x => x.Roles, role));

        return await this.collection.CountDocumentsAsync(filter, cancellationToken: ct) > 0;
    }

    public async Task<bool> HasScopeIdAsync(Guid scopeId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScope>.Filter.Eq(x => x.Id, scopeId);

        return await this.collection.CountDocumentsAsync(filter, cancellationToken: ct) > 0;
    }

    public async Task UpdateRolesAsync(Guid scopeId, UserRole[] addingRoles, UserRole[] removingRoles, CancellationToken ct = default)
    {
        var updates = new List<UpdateDefinition<KnowledgeScope>>();
        if (addingRoles is not null && addingRoles.Length > 0)
        {
            updates.Add(Builders<KnowledgeScope>.Update.AddToSetEach(x => x.Roles, addingRoles));
        }

        if (removingRoles is not null && removingRoles.Length > 0)
        {
            updates.Add(Builders<KnowledgeScope>.Update.PullAll(x => x.Roles, removingRoles));
        }

        if (updates.Count == 0)
        {
            return;
        }

        var filter = Builders<KnowledgeScope>.Filter.Eq(x => x.Id, scopeId);
        var update = Builders<KnowledgeScope>.Update.Combine(updates);
        await this.collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, UserRole role, CancellationToken ct = default)
    {
        var sortDefinition = Builders<KnowledgeScope>.Sort.Ascending("_id");
        var filterBuilder = Builders<KnowledgeScope>.Filter;
        var filter = Builders<KnowledgeScope>.Filter.And(
            Builders<KnowledgeScope>.Filter.AnyEq(x => x.Roles, role));

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync(cancellationToken: ct);
    }
}
