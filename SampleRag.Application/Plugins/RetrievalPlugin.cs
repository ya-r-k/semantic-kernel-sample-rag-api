using System.ComponentModel;
using Microsoft.SemanticKernel;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;

namespace SampleRag.Application.Plugins;

public class RetrievalPlugin(IVectorRepository<DocumentChunk> chunkRepository)
{
    [KernelFunction("RetrieveRelevantChunks")]
    [Description("Retrieves internal knowledge from document chunks for complex user queries")]
    public Task<IEnumerable<DocumentChunk>> RetrieveRelevantChunksAsync(
        [Description("ai-decomposed aspect of complex user request - NOT raw user input")] string subQuery)
    {
        //return await chunkRepository.RetrieveChunksAsync(query, topK);

        return Task.FromResult(new DocumentChunk[]
        {
            new ()
            {
                Id = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
            },
            new ()
            {
                Id = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
            },
            new ()
            {
                Id = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
            },
        }.AsEnumerable());
    }
}
