namespace SampleRag.Domain.Interfaces.Services;

/// <summary>
/// Validates scope access for the current user. Used on upload, chat create, and messages.
/// </summary>
public interface IScopeAccessService
{
    Task<bool> CanUseScopeAsync(Guid scopeId, string userId, CancellationToken ct = default);
}
