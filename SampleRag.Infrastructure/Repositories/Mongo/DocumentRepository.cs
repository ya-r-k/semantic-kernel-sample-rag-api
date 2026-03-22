using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class DocumentRepository(IMongoDatabase database) : MongoBaseRepository<Document>(database), IFilterRepository<Guid, Document, GetDocumentsByModel>
{
    public async Task<IEnumerable<Document>> GetBatchByAsync(GetDocumentsByModel filterModel)
    {
        var sortDefinition = Builders<Document>.Sort.Ascending("_id");
        var filterBuilder = Builders<Document>.Filter;
        var filter = Builders<Document>.Filter.Empty;

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        if (filterModel.ScopeId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.ScopeId == filterModel.ScopeId.Value);
        }

        if (filterModel.IsChunked.HasValue)
        {
            filter &= filterBuilder.Where(x => x.IsChunked == filterModel.IsChunked.Value);
        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync();
    }
}
