using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class KnowledgeScopeUserService(
    IKnowledgeScopeRepository scopeRepository,
    IKnowledgeScopeUserRepository scopeUserRepository) : IKnowledgeScopeUserService
{
    public async Task<bool> HasAccessAsync(Guid scopeId, string userId, CancellationToken ct = default)
    {
        return await scopeUserRepository.HasAccessAsync(scopeId, userId, ct);
    }

    public async Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, CancellationToken ct = default)
    {
        return await scopeRepository.GetBatchByAsync(filterModel);
    }

    public async Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, string userId, CancellationToken ct = default)
    {
        var scopesIds = await scopeUserRepository.GetScopeIdsForUserAsync(filterModel, userId);

        return await scopeRepository.GetByIdsAsync([.. scopesIds]);
    }
}
