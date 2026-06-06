using Mapster;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Enums;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class KnowledgeScopeService(
    IKnowledgeScopeRepository scopeRepository) : IKnowledgeScopeService
{
    public async Task<bool> HasAccessAsync(Guid scopeId, UserRole role, CancellationToken ct = default)
    {
        return await scopeRepository.HasAccessAsync(scopeId, role, ct);
    }

    public async Task<bool> HasScopeIdAsync(Guid scopeId, CancellationToken ct = default)
    {
        return await scopeRepository.HasScopeIdAsync(scopeId, ct);
    }

    public async Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, UserRole role, CancellationToken ct = default)
    {
        if (role is UserRole.Admin or UserRole.SuperAdmin)
        {
            return await scopeRepository.GetBatchByAsync(filterModel);
        }

        return await scopeRepository.GetBatchByAsync(filterModel, role, ct);
    }

    public async Task<IEnumerable<KnowledgeScope>> AddAsync(IEnumerable<CreateScopeRequest> items, CancellationToken ct = default)
    {
        return await scopeRepository.AddAsync(items.Adapt<KnowledgeScope[]>(), ct);
    }

    public async Task PartialUpdateAsync(Guid scopeId, UpdateScopeRequest request, CancellationToken ct)
    {
        await scopeRepository.PartialUpdateAsync(scopeId, request, ct);
    }

    public async Task RemoveByIdsAsync(Guid[] ids, CancellationToken ct = default)
    {
        await scopeRepository.RemoveByIdsAsync(ids, ct);
    }
}
