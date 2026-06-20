using SampleRag.Domain.Entities;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Repositories;

public interface IDocumentRepository : IFilterRepository<Guid, Document, GetDocumentsByModel>
{
    Task RecalculateIndexPercentageAsync(Guid[] documentsIds);
}
