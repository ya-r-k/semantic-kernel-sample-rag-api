using System.Linq.Expressions;
using SampleRag.Domain.Models;

namespace SampleRag.Application.Interfaces;

public interface IRepository<TId, TModel>
    where TId : unmanaged
    where TModel : Entity<TId>
{
    Task<IEnumerable<TModel>> AddAsync(params TModel[] items);

    Task UpdateAsync(params TModel[] items);

    Task RemoveByIdsAsync(params TId[] ids);

    Task<IEnumerable<TModel>> GetByIdsAsync(params TId[] ids);

    Task<IEnumerable<TModel>> GetBatchByAsync(Expression<Func<TModel, bool>>? predicate, int? batchSize);
}
