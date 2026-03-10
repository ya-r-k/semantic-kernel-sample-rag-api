using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces;

/// <summary>
/// Repository for scope-user access. Enforces (ScopeId, UserId) uniqueness.
/// </summary>
public interface IKnowledgeScopeUserRepository
{
    Task<bool> HasAccessAsync(Guid scopeId, string userId, CancellationToken ct = default);

    Task<IEnumerable<KnowledgeScopeUser>> AddAsync(KnowledgeScopeUser[] items, CancellationToken ct = default);

    Task RemoveUserAsync(Guid scopeId, string[] usersId, CancellationToken ct = default);

    Task<IEnumerable<Guid>> GetScopeIdsForUserAsync(GetBatchByModel filterModel, string userId, CancellationToken ct = default);
}
