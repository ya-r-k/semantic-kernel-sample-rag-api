using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces;

public interface IKnowledgeScopeRepository : IRepository<Guid, KnowledgeScope>
{
    Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel);
}
