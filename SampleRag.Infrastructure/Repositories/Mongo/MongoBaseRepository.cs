using System.Linq.Expressions;
using MongoDB.Driver;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Infrastructure.Repositories.Mongo;

public class MongoBaseRepository<TEntity>(IMongoDatabase database) : IRepository<Guid, TEntity>
    where TEntity : IEntity<Guid>
{
    protected readonly IMongoCollection<TEntity> collection = database.GetCollection<TEntity>(typeof(TEntity).Name, new MongoCollectionSettings
    {
        AssignIdOnInsert = true,
    });

    public async Task<IEnumerable<TEntity>> AddAsync(TEntity[] items, CancellationToken ct = default)
    {
        var addedItems = new List<TEntity>(items);

        try
        {
            await this.collection.InsertManyAsync(addedItems, new InsertManyOptions { IsOrdered = true }, ct);
        }
        catch (MongoBulkWriteException<TEntity> ex)
        {
            addedItems = [.. addedItems.Except(ex.WriteErrors.Select(err => items[err.Index]))];
        }

        return addedItems;
    }

    public async Task UpdateAsync(TEntity[] items, CancellationToken ct = default)
    {
        var bulkOps = items.Select(item =>
            new ReplaceOneModel<TEntity>(
                Builders<TEntity>.Filter.Eq(e => e.Id, item.Id),
                item)).ToList();

        await this.collection.BulkWriteAsync(bulkOps, cancellationToken: ct);
    }

    public async Task SetFieldValueAsync<T>(Expression<Func<TEntity, T>> fieldSelector, T value)
        where T : unmanaged
    {
        var filter = Builders<TEntity>.Filter.Empty;
        var update = Builders<TEntity>.Update.Set(fieldSelector, value);

        await this.collection.UpdateManyAsync(filter, update);
    }

    public async Task<IEnumerable<TEntity>> GetBatchByAsync(Expression<Func<TEntity, bool>>? predicate, int? batchSize, CancellationToken ct = default)
    {
        var sortDefinition = Builders<TEntity>.Sort.Ascending("_id");
        var filterBuilder = Builders<TEntity>.Filter;
        var filter = Builders<TEntity>.Filter.Empty;

        if (predicate != null)
        {
            filter &= filterBuilder.Where(predicate);
        }

        var query = this.collection.Find(filter).Sort(sortDefinition);

        if (batchSize.HasValue)
        {
            query = query.Limit(batchSize.Value);
        }

        return await query.ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<TEntity>> GetByIdsAsync(Guid[] ids, CancellationToken ct = default)
    {
        var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

        return await this.collection.Find(filter).ToListAsync(cancellationToken: ct);
    }

    public async Task RemoveByIdsAsync(Guid[] ids, CancellationToken ct = default)
    {
        var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

        await this.collection.DeleteManyAsync(filter, ct);
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        await this.collection.Database.DropCollectionAsync(typeof(TEntity).Name, ct);
    }
}
