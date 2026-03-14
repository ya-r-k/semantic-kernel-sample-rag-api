using SampleRag.Domain.Entities;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Domain.Interfaces.Services;

public interface IFeedbackService
{
    Task UpsertFeedbackAsync(Feedback item, CancellationToken ct = default);

    Task<IEnumerable<Feedback>> GetFeedbackAsync(GetFeedbackByModel filterModel, CancellationToken ct = default);
}
