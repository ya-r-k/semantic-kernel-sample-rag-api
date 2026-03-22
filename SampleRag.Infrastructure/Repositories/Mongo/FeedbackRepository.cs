using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class FeedbackRepository(IMongoDatabase database) : MongoBaseRepository<Feedback>(database), IFilterRepository<Guid, Feedback, GetFeedbackByModel>
{
    public async Task<IEnumerable<Feedback>> GetBatchByAsync(GetFeedbackByModel filterModel)
    {
        var sortDefinition = Builders<Feedback>.Sort.Ascending("_id");
        var filterBuilder = Builders<Feedback>.Filter;
        var filter = Builders<Feedback>.Filter.Empty;

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        if (filterModel.MessageId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.MessageId == filterModel.MessageId.Value);
        }

        if (filterModel.IsLike.HasValue)
        {
            filter &= filterBuilder.Where(x => x.IsLike == filterModel.IsLike.Value);
        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync();
    }
}
