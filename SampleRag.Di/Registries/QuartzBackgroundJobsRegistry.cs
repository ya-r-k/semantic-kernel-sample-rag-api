using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SampleRag.Application.Jobs;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.Di.Registries;

public static class QuartzBackgroundJobsRegistry
{
    public static void ConfigureQuartzJobs(this IServiceCollection services, DbSettings dbSettings, DocumentsJobsSettings jobsSettings)
    {
        services.AddSingleton(jobsSettings);

        services.AddQuartz(q =>
        {
            q.AddJob<DocumentChunkingJob>(options => options.WithIdentity("chunk-documents"));
            q.AddTrigger(options => options
                .ForJob("chunk-documents")
                .WithIdentity("chunk-documents-trigger")
                .WithCronSchedule("0 0/5 11-23,0-7 * * ?")
                .WithDescription("Чанкинг документов каждые 30 сек с 21:00 до 08:00"));

            q.AddJob<ChunkVectorizationJob>(options => options.WithIdentity("vectorize-chunks"));
            q.AddTrigger(options => options
                .ForJob("vectorize-chunks")
                .WithIdentity("vectorize-chunks-trigger")
                .WithCronSchedule("0 0/2 11-23,0-7 * * ?")
                .WithDescription("Ночное задание каждые 2 минуты"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}
