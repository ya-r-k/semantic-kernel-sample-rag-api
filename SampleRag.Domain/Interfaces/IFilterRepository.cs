using SampleRag.Domain.Models.Abstractions;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces;

public interface IFilterRepository<TId, TModel, TFilterModel> : IRepository<TId, TModel>
    where TId : unmanaged
    where TModel : IEntity<TId>
    where TFilterModel : GetBatchByModel
{
    Task<IEnumerable<TModel>> GetBatchByAsync(TFilterModel model);
}
