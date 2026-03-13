using Mapster;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class KnowledgeScopeService(
    IFilterRepository<Guid, KnowledgeScope, GetBatchByModel> scopeRepository,
    IKnowledgeScopeUserRepository scopeUserRepository) : IKnowledgeScopeService
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
        var scopesIds = await scopeUserRepository.GetScopeIdsForUserAsync(filterModel, userId, ct);

        return await scopeRepository.GetByIdsAsync([.. scopesIds]);
    }

    public async Task<IEnumerable<KnowledgeScope>> AddAsync(IEnumerable<CreateScopeRequest> items, CancellationToken ct = default)
    {
        var scopes = items.Adapt<KnowledgeScope[]>();

        return await scopeRepository.AddAsync(scopes);
    }

    public async Task AddUsersAsync(Guid id, string[] usersIds, CancellationToken ct)
    {
        var items = usersIds.Select(x => new KnowledgeScopeUser
        {
            ScopeId = id,
            UserId = x,
        }).ToArray();

        await scopeUserRepository.AddAsync(items, ct);
    }

    public async Task RemoveByIds(Guid[] ids, CancellationToken ct = default)
    {
        await scopeRepository.RemoveByIdsAsync(ids);
    }
}
