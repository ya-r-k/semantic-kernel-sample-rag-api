using SampleRag.Domain.Models.Abstractions;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces;

public interface IFilterRepository<TId, TEntity, TFilterModel> : IRepository<TId, TEntity>
    where TId : unmanaged
    where TEntity : IEntity<TId>
    where TFilterModel : GetBatchByModel
{
    Task<IEnumerable<TEntity>> GetBatchByAsync(TFilterModel filterModel);
}
