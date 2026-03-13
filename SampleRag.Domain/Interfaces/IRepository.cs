using SampleRag.Domain.Models.Abstractions;
using System.Linq.Expressions;

namespace SampleRag.Domain.Interfaces;

public interface IRepository<TId, TEntity>
    where TId : unmanaged
    where TEntity : IEntity<TId>
{
    Task<IEnumerable<TEntity>> AddAsync(TEntity[] items, CancellationToken ct = default);

    Task UpdateAsync(TEntity[] items, CancellationToken ct = default);

    Task SetFieldValueAsync<T>(Expression<Func<TEntity, T>> fieldSelector, T value)
        where T : unmanaged;

    Task RemoveByIdsAsync(TId[] ids, CancellationToken ct = default);

    Task ClearAsync(CancellationToken ct = default);

    Task<IEnumerable<TEntity>> GetByIdsAsync(TId[] ids, CancellationToken ct = default);

    Task<IEnumerable<TEntity>> GetBatchByAsync(Expression<Func<TEntity, bool>>? predicate, int? batchSize, CancellationToken ct = default);
}
