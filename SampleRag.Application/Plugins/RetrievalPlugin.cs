using System.ComponentModel;
using Microsoft.SemanticKernel;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Services;

namespace SampleRag.Application.Plugins;

public class RetrievalPlugin(IDocumentChunkService chunkService)
{
    [KernelFunction("RetrieveRelevantChunks")]
    [Description("Retrieves internal knowledge from document chunks for complex user queries")]
    public async Task<IEnumerable<DocumentChunk>> RetrieveRelevantChunksAsync(
        [Description("ai-decomposed aspect of complex user request - NOT raw user input")] string subQuery)
    {
        var sources = await chunkService.RetrieveChunksAsync(subQuery);

        return sources;
    }
}
