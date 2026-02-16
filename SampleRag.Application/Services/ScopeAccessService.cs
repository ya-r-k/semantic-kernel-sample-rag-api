using SampleRag.Application.Interfaces;
using SampleRag.Application.Interfaces.Services;

namespace SampleRag.Application.Services;

public class ScopeAccessService(IScopeUserRepository scopeUserRepository) : IScopeAccessService
{
    public Task<bool> CanUseScopeAsync(Guid scopeId, string userId, CancellationToken ct = default) =>
        scopeUserRepository.HasAccessAsync(scopeId, userId, ct);
}
