using SampleRag.Domain.Entities;
using SampleRag.Domain.Models.Enums;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for scope-user access. Enforces (ScopeId, UserId) uniqueness.
/// </summary>
public interface IKnowledgeScopeRepository : IFilterRepository<Guid, KnowledgeScope, GetBatchByModel>
{
    Task<bool> HasAccessAsync(Guid scopeId, UserRole role, CancellationToken ct = default);

    Task<bool> HasScopeIdAsync(Guid scopeId, CancellationToken ct = default);

    Task PartialUpdateAsync(Guid scopeId, UpdateScopeRequest request, CancellationToken ct = default);

    Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, UserRole role, CancellationToken ct = default);

    Task RecalculateIndexPercentageAsync(Guid[] documentsIds);
}
