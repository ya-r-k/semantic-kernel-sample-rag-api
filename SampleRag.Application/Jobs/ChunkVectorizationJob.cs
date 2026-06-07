using Quartz;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Repositories;
using SampleRag.Domain.Models.Configs;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Jobs;

[DisallowConcurrentExecution]
public class ChunkVectorizationJob(
    DocumentsJobsSettings settings,
    IFilterRepository<Guid, DocumentChunk, GetDocumentChunksByModel> dbRepository,
    IDocumentRepository documentRepository,
    IKnowledgeScopeRepository scopeRepository,
    IVectorRepository<DocumentChunk> vectorRepository) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var chunks = await dbRepository.GetBatchByAsync(new GetDocumentChunksByModel
        {
            BatchSize = settings.ChunksBatchSize,
            IsVectorized = false,
        });

        if (chunks.Any())
        {
            var documentsIds = chunks.Select(x => x.DocumentId)
                .Distinct()
                .ToArray();

            await vectorRepository.UpsertChunksAsync([.. chunks]);
            foreach (var chunk in chunks)
            {
                chunk.IsVectorized = true;
            }

            await dbRepository.UpdateAsync([.. chunks]);

            await documentRepository.RecalculateIndexPercentageAsync(documentsIds);
            await scopeRepository.RecalculateIndexPercentageAsync(documentsIds);
        }
    }
}
