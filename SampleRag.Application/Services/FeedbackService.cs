using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class FeedbackService(IFilterRepository<Guid, Feedback, GetFeedbackByModel> feedbackRepository) : IFeedbackService
{
    public async Task<IEnumerable<Feedback>> GetFeedbackAsync(GetFeedbackByModel filterModel, CancellationToken ct = default)
    {
        return await feedbackRepository.GetBatchByAsync(filterModel);
    }

    public async Task UpsertFeedbackAsync(Feedback item, CancellationToken ct = default)
    {
        var existing = await feedbackRepository.GetBatchByAsync(x => x.MessageId == item.MessageId && x.UserId == item.UserId, 1, ct);
        var feedback = existing.FirstOrDefault();
        if (feedback is null)
        {
            await feedbackRepository.AddAsync([item], ct);
        }
        else
        {
            feedback.IsLike = item.IsLike;
            await feedbackRepository.UpdateAsync([feedback], ct);
        }
    }
}
