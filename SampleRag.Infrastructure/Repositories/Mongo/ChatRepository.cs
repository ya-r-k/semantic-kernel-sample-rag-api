using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class ChatRepository(IMongoDatabase database) : MongoBaseRepository<Chat>(database), IFilterRepository<Guid, Chat, GetChatsByModel>
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
}
