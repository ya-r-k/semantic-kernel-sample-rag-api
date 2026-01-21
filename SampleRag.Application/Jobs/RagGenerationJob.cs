using Microsoft.AspNetCore.SignalR;
using Quartz;
using SampleRagAPI.RagAPI.Hubs;

namespace SampleRag.API.Jobs;

[DisallowConcurrentExecution]
public class RagGenerationJob(ILogger<RagGenerationJob> logger,
    IServiceProvider serviceProvider,
    IHubContext<RagMessagesHub> hubContext) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;

        logger.LogInformation("Starting RAG job {JobId} for user {UserId}", jobData.JobId, jobData.UserId);

        throw new NotImplementedException();
    }
}
