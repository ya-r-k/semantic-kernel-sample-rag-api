using Microsoft.SemanticKernel;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;
using System.ComponentModel;

namespace SampleRag.Application.KernelFunctions.Plugins;

public class RetrievalPlugin(IVectorRepository<DocumentChunk> chunkRepository)
{
    [KernelFunction("GetRelevantChunks")]
    [Description("Ищет релевантные чанки документов по запросу пользователя для RAG")]
    public async Task<IEnumerable<DocumentChunk>> GetRelevantChunksAsync(
        [Description("Запрос пользователя")] string query,
        [Description("Количество чанков")] int topK = 5)
    {
        return await chunkRepository.RetrieveChunksAsync(query, topK);
    }
}
