using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class KnowledgeScopeRepository(IMongoDatabase database) : MongoBaseRepository<KnowledgeScope>(database), IFilterRepository<Guid, KnowledgeScope, GetBatchByModel>
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
}
