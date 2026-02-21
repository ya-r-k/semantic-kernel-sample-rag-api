using Quartz;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;

namespace SampleRag.Application.Jobs;

public class ChunkVectorizationJob(
    IDocumentChunkService service,
    IVectorRepository<DocumentChunk> vectorRepository) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var chunks = await service.GetBatchByAsync(x => !x.IsVectorized, 1000);

        await vectorRepository.UpsertChunksAsync([.. chunks]);
    }
}
