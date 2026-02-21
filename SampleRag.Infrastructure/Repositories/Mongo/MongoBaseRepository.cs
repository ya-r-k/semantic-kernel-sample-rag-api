using MongoDB.Driver;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Abstractions;
using System.Linq.Expressions;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class MongoBaseRepository<TModel>(IMongoDatabase database) : IRepository<Guid, TModel>
    where TModel : IEntity<Guid>
{
    protected readonly IMongoCollection<TModel> _collection = database.GetCollection<TModel>(typeof(TModel).Name, new MongoCollectionSettings 
    {
        AssignIdOnInsert = true,
    });

    public async Task<IEnumerable<TModel>> AddAsync(TModel[] items)
    {
        var addedItems = new List<TModel>(items);

        try
        {
            await _collection.InsertManyAsync(addedItems, new InsertManyOptions { IsOrdered = false });
        }
        catch (MongoBulkWriteException<TModel> ex)
        {
            addedItems = [.. addedItems.Except(ex.WriteErrors.Select(err => items[err.Index]))];
        }

        return addedItems;
    }

    public async Task UpdateAsync(TModel[] items)
    {
        var bulkOps = items.Select(item =>
            new ReplaceOneModel<TModel>(
                Builders<TModel>.Filter.Eq(e => e.Id, item.Id),
                item
            )).ToList();

        await _collection.BulkWriteAsync(bulkOps);
    }

    public async Task<IEnumerable<TModel>> GetBatchByAsync(Expression<Func<TModel, bool>>? predicate, int? batchSize)
    {
        var sortDefinition = Builders<TModel>.Sort.Ascending("_id");
        var builder = Builders<TModel>.Filter;
        var filter = Builders<TModel>.Filter.Empty;

        if (predicate != null)
        {
            filter &= builder.Where(predicate);
        }

        var query = _collection.Find(filter).Sort(sortDefinition);

        if (batchSize.HasValue)
        {
            query = query.Limit(batchSize.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<TModel>> GetByIdsAsync(params Guid[] ids)
    {
        var filter = Builders<TModel>.Filter.In(x => x.Id, ids);

        return await _collection.Find(filter).ToListAsync();
    }

    public async Task RemoveByIdsAsync(params Guid[] ids)
    {
        var filter = Builders<TModel>.Filter.In(x => x.Id, ids);

        await _collection.DeleteManyAsync(filter);
    }
}
