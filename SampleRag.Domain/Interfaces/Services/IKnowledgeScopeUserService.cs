using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

/// <summary>
/// Validates scope access for the current user. Used on upload, chat create, and messages.
/// </summary>
public interface IKnowledgeScopeUserService
{
    Task<bool> HasAccessAsync(Guid scopeId, string userId, CancellationToken ct = default);

    Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, CancellationToken ct = default);

    Task<IEnumerable<KnowledgeScope>> GetBatchByAsync(GetBatchByModel filterModel, string userId, CancellationToken ct = default);
}
