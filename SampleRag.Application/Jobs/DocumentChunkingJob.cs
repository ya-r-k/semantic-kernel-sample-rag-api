using Quartz;
using SampleRag.Domain.Interfaces.Services;

namespace SampleRag.Application.Jobs;

public class DocumentChunkingJob(
    IDocumentChunkService service,
    IDocumentService documentService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var documents = await documentService.GetBatchByAsync(x => !x.IsChunked, 100);
        foreach (var document in documents) 
        {
            document.IsChunked = true;
            await service.ChunkAsync(document);
        }

        await documentService.UpdateAsync([.. documents]);
    }
}
