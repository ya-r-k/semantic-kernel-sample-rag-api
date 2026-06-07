using MongoDB.Bson;
using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Repositories;
using SampleRag.Domain.Models.Enums;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class KnowledgeScopeRepository(IMongoDatabase database) : MongoBaseRepository<KnowledgeScope>(database), IKnowledgeScopeRepository
{
    private readonly IMongoCollection<DocumentChunk> chunksCollection = database.GetCollection<DocumentChunk>(nameof(DocumentChunk));

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

    public async Task<bool> HasAccessAsync(Guid scopeId, UserRole role, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScope>.Filter.And(
            Builders<KnowledgeScope>.Filter.Eq(x => x.Id, scopeId),
            Builders<KnowledgeScope>.Filter.AnyEq(x => x.Roles, role));

        return await this.collection.CountDocumentsAsync(filter, cancellationToken: ct) > 0;
    }

    public async Task<bool> HasScopeIdAsync(Guid scopeId, CancellationToken ct = default)
    {
        var filter = Builders<KnowledgeScope>.Filter.Eq(x => x.Id, scopeId);

        return await this.collection.CountDocumentsAsync(filter, cancellationToken: ct) > 0;
    }

    public async Task PartialUpdateAsync(Guid scopeId, UpdateScopeRequest request, CancellationToken ct = default)
    {
        var updates = new List<UpdateDefinition<KnowledgeScope>>();
        if (!string.IsNullOrEmpty(request.Name))
        {
            updates.Add(Builders<KnowledgeScope>.Update.Set(x => x.Name, request.Name));
        }

        if (request.AddingRoles is not null && request.AddingRoles.Length > 0)
        {
            updates.Add(Builders<KnowledgeScope>.Update.AddToSetEach(x => x.Roles, request.AddingRoles));
        }

        if (request.RemovingRoles is not null && request.RemovingRoles.Length > 0)
        {
            updates.Add(Builders<KnowledgeScope>.Update.PullAll(x => x.Roles, request.RemovingRoles));
        }

        if (updates.Count == 0)
        {
            return;
        }

        var filter = Builders<KnowledgeScope>.Filter.Eq(x => x.Id, scopeId);
        var update = Builders<KnowledgeScope>.Update.Combine(updates);
        await this.collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, UserRole role, CancellationToken ct = default)
    {
        var sortDefinition = Builders<KnowledgeScope>.Sort.Ascending("_id");
        var filterBuilder = Builders<KnowledgeScope>.Filter;
        var filter = Builders<KnowledgeScope>.Filter.And(
            Builders<KnowledgeScope>.Filter.AnyEq(x => x.Roles, role));

        if (filterModel.LastId.HasValue)
        {
            filter &= filterBuilder.Where(x => x.Id > filterModel.LastId.Value);
        }

        var query = this.collection.Find(filter)
            .Sort(sortDefinition)
            .Limit(filterModel.BatchSize);

        return await query.ToListAsync(cancellationToken: ct);
    }

    public async Task RecalculateIndexPercentageAsync(Guid[] documentsIds)
    {
        var pipeline = new BsonDocument[]
        {
           new ("$match", new BsonDocument
           {
               {
                   "DocumentId",
                   new BsonDocument("$in", new BsonArray(documentsIds
                       .Select(id => new BsonBinaryData(id, GuidRepresentation.Standard))
                       .ToList()))
               },
           }),
           new ("$group", new BsonDocument
           {
               { "_id", "$DocumentId" },
               { "TotalChunks", new BsonDocument("$sum", 1) },
               {
                   "VectorizedChunks",
                   new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                   {
                       new BsonDocument("$eq", new BsonArray { "$IsVectorized", true }),
                       1,
                       0,
                   }))
               },
           }),
           new ("$lookup", new BsonDocument
           {
               { "from", "Document" },
               { "localField", "_id" },
               { "foreignField", "_id" },
               { "as", "DocumentInfo" },
           }),
           new ("$unwind", "$DocumentInfo"),
           new ("$group", new BsonDocument
           {
               { "_id", "$DocumentInfo.ScopeId" },
               { "TotalChunksInScope", new BsonDocument("$sum", "$TotalChunks") },
               { "VectorizedChunksInScope", new BsonDocument("$sum", "$VectorizedChunks") },
               { "DocumentsCount", new BsonDocument("$sum", 1) },
           }),
           new ("$project", new BsonDocument
           {
               { "ScopeId", "$_id" },
               {
                   "IndexPercentage",
                   new BsonDocument("$cond", new BsonArray
                   {
                       new BsonDocument("$gt", new BsonArray { "$TotalChunksInScope", 0 }),
                       new BsonDocument("$multiply", new BsonArray
                       {
                           new BsonDocument("$divide", new BsonArray { "$VectorizedChunksInScope", "$TotalChunksInScope" }),
                           100.0
                       }),
                       0.0,
                   })
               },
               { "DocumentsCount", 1 },
               { "_id", 0 },
           }),
        };

        var scopeAggregation = await chunksCollection.Aggregate<BsonDocument>(pipeline)
            .ToListAsync();

        var updates = scopeAggregation
            .Select(result => new UpdateOneModel<KnowledgeScope>(
                Builders<KnowledgeScope>.Filter.Eq(s => s.Id, result["ScopeId"].AsGuid),
                Builders<KnowledgeScope>.Update.Set(s => s.IndexPercentage, result["IndexPercentage"].AsDouble)))
            .ToList();

        if (updates.Count > 0)
        {
            await collection.BulkWriteAsync(updates);
        }
    }

    public async Task RecalculateDocumentsCountAsync(Guid[] scopesIds)
    {
        var filter = Builders<KnowledgeScope>.Filter.In(x => x.Id, scopesIds);
        var pipeline = new BsonDocument[]
        {
            new ("$match", new BsonDocument
            {
                {
                    "_id",
                    new BsonDocument("$in", new BsonArray(scopesIds
                        .Select(id => new BsonBinaryData(id, GuidRepresentation.Standard))
                        .ToList()))
                },
            }),
            new ("$lookup", new BsonDocument
            {
                { "from", "Document" },
                { "localField", "_id" },
                { "foreignField", "ScopeId" },
                { "as", "ScopeDocuments" },
            }),
            new ("$addFields", new BsonDocument
            {
                { "DocumentsCount", new BsonDocument("$size", "$ScopeDocuments") },
            }),
            new ("$project", new BsonDocument
            {
                { "_id", 1 },
                { "DocumentsCount", 1 },
            }),
        };

        var scopeAggregation = await collection.Aggregate<BsonDocument>(pipeline)
            .ToListAsync();

        var updates = scopeAggregation
            .Select(result => new UpdateOneModel<KnowledgeScope>(
                Builders<KnowledgeScope>.Filter.Eq(s => s.Id, result["_id"].AsGuid),
                Builders<KnowledgeScope>.Update.Set(s => s.DocumentsCount, result["DocumentsCount"].AsInt32)))
            .ToList();

        if (updates.Count > 0)
        {
            await collection.BulkWriteAsync(updates);
        }
    }
}
