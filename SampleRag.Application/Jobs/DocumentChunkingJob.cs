using Quartz;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Configs;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Jobs;

[DisallowConcurrentExecution]
public class DocumentChunkingJob(
    DocumentsJobsSettings settings,
    IDocumentChunkService documentChunkService,
    IDocumentService documentService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var documents = await documentService.GetBatchByAsync(new GetDocumentsByModel
        {
            BatchSize = settings.DocumentsBatchSize,
            IsChunked = false,
        });

        if (documents.Any())
        {
            foreach (var document in documents)
            {
                document.IsChunked = true;
                await documentChunkService.ChunkAsync(document);
            }

            await documentService.UpdateAsync([.. documents]);
        }
    }
}
