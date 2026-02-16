namespace SampleRag.Application.Interfaces;

/// <summary>
/// Repository for scope-user access. Enforces (ScopeId, UserId) uniqueness.
/// </summary>
public interface IScopeUserRepository
{
    Task<bool> HasAccessAsync(Guid scopeId, string userId, CancellationToken ct = default);

    Task AddUserAsync(Guid scopeId, string userId, CancellationToken ct = default);

    Task RemoveUserAsync(Guid scopeId, string userId, CancellationToken ct = default);

    Task<IEnumerable<Guid>> GetScopeIdsForUserAsync(string userId, CancellationToken ct = default);
}
