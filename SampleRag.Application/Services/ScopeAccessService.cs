using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;

namespace SampleRag.Application.Services;

public class ScopeAccessService(IKnowledgeGroupUserRepository scopeUserRepository) : IScopeAccessService
{
    public Task<bool> CanUseScopeAsync(Guid scopeId, string userId, CancellationToken ct = default) =>
        scopeUserRepository.HasAccessAsync(scopeId, userId, ct);
}
