using MongoDB.Driver;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class ChatRepository(IMongoDatabase database) : MongoBaseRepository<Chat>(database), IFilterRepository<Guid, Chat, GetChatsByModel>
{
    public async Task<IEnumerable<Chat>> GetBatchByAsync(GetChatsByModel model)
    {
        var sortDefinition = Builders<Chat>.Sort.Ascending("_id");
        var filterBuilder = Builders<Chat>.Filter;
        var filter = Builders<Chat>.Filter.Empty;

        if (model.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > model.LastId.Value);
        }

        if (model.ScopeId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ScopeId == model.ScopeId.Value);
        }

        var query = collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(model.BatchSize);

        return await collection.Find(filter).ToListAsync();
    }
}
