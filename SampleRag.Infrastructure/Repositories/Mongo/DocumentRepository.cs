using MongoDB.Driver;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class DocumentRepository(IMongoDatabase database) : MongoBaseRepository<Document>(database), IFilterRepository<Guid, Document, GetDocumentsByModel>
{
    public async Task<IEnumerable<Document>> GetBatchByAsync(GetDocumentsByModel model)
    {
        var sortDefinition = Builders<Document>.Sort.Ascending("_id");
        var filterBuilder = Builders<Document>.Filter;
        var filter = Builders<Document>.Filter.Empty;

        if (model.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > model.LastId.Value);
        }

        if (model.ScopeId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ScopeId == model.ScopeId.Value);
        }

        if (model.IsChunked.HasValue)
        {
            filter &= filterBuilder.Where(x => x.IsChunked == model.IsChunked.Value);
        }

        var query = collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(model.BatchSize);

        return await collection.Find(filter).ToListAsync();
    }
}
