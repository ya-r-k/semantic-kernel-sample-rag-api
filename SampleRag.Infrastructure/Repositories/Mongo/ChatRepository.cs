using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class ChatRepository(IMongoDatabase database) : MongoBaseRepository<Chat>(database), IFilterRepository<Guid, Chat, GetChatsByModel>
{
    public async Task<IEnumerable<Chat>> GetBatchByAsync(GetChatsByModel filterModel)
    {
        var filterBuilder = Builders<Chat>.Filter;
        var filter = Builders<Chat>.Filter.Empty;

        if (filterModel.ScopeId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ScopeId == filterModel.ScopeId.Value);
        }

        if (filterModel.LastId.HasValue)
        {
            var anchor = await this.collection
                .Find(filterBuilder.Eq(x => x.Id, filterModel.LastId.Value))
                .FirstOrDefaultAsync();

            if (anchor is not null)
            {
                if (anchor.LastUpdatedAt.HasValue)
                {
                    filter &= filterBuilder.Or(
                        filterBuilder.Lt(x => x.LastUpdatedAt, anchor.LastUpdatedAt.Value),
                        filterBuilder.And(
                            filterBuilder.Eq(x => x.LastUpdatedAt, anchor.LastUpdatedAt.Value),
                            filterBuilder.Lt(x => x.Id, anchor.Id)));
                }
                else
                {
                    filter &= filterBuilder.And(
                        filterBuilder.Eq(x => x.LastUpdatedAt, null),
                        filterBuilder.Lt(x => x.Id, anchor.Id));
                }
            }
        }

        var sortDefinition = Builders<Chat>.Sort
            .Descending(x => x.LastUpdatedAt)
            .Descending(x => x.Id);

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync();
    }
}
