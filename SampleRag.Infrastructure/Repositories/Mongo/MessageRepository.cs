using MongoDB.Driver;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class MessageRepository(IMongoDatabase database) : MongoBaseRepository<Message>(database), IFilterRepository<Guid, Message, GetMessagesByModel>
{
    public async Task<IEnumerable<Message>> GetBatchByAsync(GetMessagesByModel model)
    {
        var sortDefinition = Builders<Message>.Sort.Ascending("_id");
        var filterBuilder = Builders<Message>.Filter;
        var filter = Builders<Message>.Filter.Empty;

        if (model.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > model.LastId.Value);
        }

        if (model.ChatId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ChatId == model.ChatId.Value);
        }

        var query = collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(model.BatchSize);

        return await collection.Find(filter).ToListAsync();
    }
}
