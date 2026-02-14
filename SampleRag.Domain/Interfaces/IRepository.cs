using SampleRag.Domain.Models.Abstractions;
using System.Linq.Expressions;

namespace SampleRag.Application.Interfaces;

public interface IRepository<TId, TModel>
    where TId : unmanaged
    where TModel : IEntity<TId>
{
    Task<IEnumerable<TModel>> AddAsync(params TModel[] items);

    Task UpdateAsync(params TModel[] items);

    Task RemoveByIdsAsync(params TId[] ids);

    Task<IEnumerable<TModel>> GetByIdsAsync(params TId[] ids);

    Task<IEnumerable<TModel>> GetBatchByAsync(Expression<Func<TModel, bool>>? predicate, int? batchSize);
}
