using Quartz;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Jobs;

public class ChunkVectorizationJob(
    IFilterRepository<Guid, DocumentChunk, GetDocumentChunksByModel> dbRepository,
    IVectorRepository<DocumentChunk> vectorRepository) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var chunks = await dbRepository.GetBatchByAsync(new GetDocumentChunksByModel
        {
            BatchSize = 100,
            IsVectorized = false,
        });

        await vectorRepository.UpsertChunksAsync([.. chunks]);

        foreach (var chunk in chunks)
        {
            chunk.IsVectorized = true;
        }

        await dbRepository.UpdateAsync([.. chunks]);
    }
}
