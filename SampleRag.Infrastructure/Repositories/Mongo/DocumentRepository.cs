using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Repositories;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class DocumentRepository(IMongoDatabase database) : MongoBaseRepository<Document>(database), IDocumentRepository
{
    private readonly IMongoCollection<DocumentChunk> chunksCollection = database.GetCollection<DocumentChunk>(nameof(DocumentChunk));

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

        if (filterModel.IsOutOfDate.HasValue)
        {
            filter &= filterBuilder.Where(x => x.IsOutOfDate == filterModel.IsOutOfDate.Value);

        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync();
    }

    public async Task RecalculateIndexPercentageAsync(Guid[] documentsIds)
    {
        var chunkFilter = Builders<DocumentChunk>.Filter.In(x => x.DocumentId, documentsIds);
        var documentAggregation = await chunksCollection.Aggregate()
            .Match(chunkFilter)
            .Group(
                c => c.DocumentId,
                g => new
                {
                    DocumentId = g.Key,
                    TotalChunks = g.Count(),
                    VectorizedChunks = g.Count(c => c.IsVectorized),
                })
            .Project(g => new
            {
                g.DocumentId,
                IndexPercentage = g.TotalChunks > 0 ? (double)g.VectorizedChunks / g.TotalChunks * 100 : 0,
            })
            .ToListAsync();

        var updates = documentAggregation
            .Select(result => new UpdateOneModel<Document>(
                Builders<Document>.Filter.Eq(d => d.Id, result.DocumentId),
                Builders<Document>.Update.Set(d => d.IndexPercentage, result.IndexPercentage)))
            .ToList();

        if (updates.Count > 0)
        {
            await collection.BulkWriteAsync(updates);
        }
    }
}
