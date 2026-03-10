using MongoDB.Driver;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class DocumentChunkRepository(IMongoDatabase database) : MongoBaseRepository<DocumentChunk>(database), IFilterRepository<Guid, DocumentChunk, GetDocumentChunksByModel>
{
    public async Task<IEnumerable<DocumentChunk>> GetBatchByAsync(GetDocumentChunksByModel model)
    {
        var sortDefinition = Builders<DocumentChunk>.Sort.Ascending("_id");
        var filterBuilder = Builders<DocumentChunk>.Filter;
        var filter = Builders<DocumentChunk>.Filter.Empty;

        if (model.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > model.LastId.Value);
        }

        if (model.DocumentId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.DocumentId == model.DocumentId.Value);
        }

        if (model.IsVectorized.HasValue)
        {
            filter &= filterBuilder.Where(x => x.IsVectorized == model.IsVectorized.Value);
        }

        var query = collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(model.BatchSize);

        return await collection.Find(filter).ToListAsync();
    }
}
