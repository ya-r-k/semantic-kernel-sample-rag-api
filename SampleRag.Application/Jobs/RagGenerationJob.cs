using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace SampleRag.Application.Jobs;

[DisallowConcurrentExecution]
public class RagGenerationJob(ILogger<RagGenerationJob> logger,
    IServiceProvider serviceProvider,
    IMediator mediator) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;

        throw new NotImplementedException();
    }
}
