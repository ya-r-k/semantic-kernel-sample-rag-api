using Quartz;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Jobs;

public class DocumentChunkingJob(
    IDocumentChunkService documentChunkService,
    IDocumentService documentService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var documents = await documentService.GetBatchByAsync(new GetDocumentsByModel
        {
            BatchSize = 100,
            IsChunked = false,
        });
        foreach (var document in documents)
        {
            document.IsChunked = true;
            await documentChunkService.ChunkAsync(document);
        }

        await documentService.UpdateAsync([.. documents]);
    }
}
