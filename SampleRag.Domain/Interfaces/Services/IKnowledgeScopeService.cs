using SampleRag.Domain.Entities;
using SampleRag.Domain.Models.Enums;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

/// <summary>
/// Validates scope access for the current user. Used on upload, chat create, and messages.
/// </summary>
public interface IKnowledgeScopeService
{
    Task<IEnumerable<KnowledgeScope>> AddAsync(IEnumerable<CreateScopeRequest> items, CancellationToken ct = default);

    Task PartialUpdateAsync(Guid scopeId, UpdateScopeRequest request, CancellationToken ct = default);

    Task<bool> HasAccessAsync(Guid scopeId, UserRole role, CancellationToken ct = default);

    Task<bool> HasScopeIdAsync(Guid scopeId, CancellationToken ct = default);

    Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, UserRole role, CancellationToken ct = default);

    Task RemoveByIds(Guid[] ids, CancellationToken ct = default);
}
