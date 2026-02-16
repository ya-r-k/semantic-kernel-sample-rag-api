using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.SemanticKernel.Embeddings;
using SampleRag.Application.Interfaces;
using SampleRag.Application.Interfaces.Services;

namespace SampleRag.Infrastructure.VectorStore;

#pragma warning disable CS0618 // ITextEmbeddingGenerationService is obsolete but still supported
public class DocumentChunkStore(
    IHttpClientFactory httpClientFactory,
    ITextEmbeddingGenerationService embeddingService) : IDocumentChunkStore
#pragma warning restore CS0618
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Qdrant");

    public async Task UpsertChunksAsync(IReadOnlyList<DocumentChunkInfo> chunks, CancellationToken ct = default)
    {
        if (chunks.Count == 0)
            return;

        var points = new List<object>();
        foreach (var chunk in chunks)
        {
            var embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Text, cancellationToken: ct);
            var vector = embedding.ToArray();

            var payload = new Dictionary<string, object>
            {
                ["DocumentId"] = chunk.DocumentId.ToString(),
                ["ScopeId"] = chunk.ScopeId.ToString(),
                ["PageNumber"] = chunk.PageNumber,
                ["Text"] = chunk.Text
            };
            if (chunk.ChunkIndex.HasValue)
                payload["ChunkIndex"] = chunk.ChunkIndex.Value;

            points.Add(new
            {
                id = Guid.NewGuid().ToString(),
                vector = vector,
                payload = payload
            });
        }

        var body = new { points };
        var response = await _httpClient.PutAsJsonAsync(
            $"collections/{QdrantCollectionConfig.CollectionName}/points?wait=true",
            body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveChunksByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        var filter = new
        {
            must = new[]
            {
                new { key = "DocumentId", match = new { value = documentId.ToString() } }
            }
        };
        var body = new { filter };
        var response = await _httpClient.PostAsJsonAsync(
            $"collections/{QdrantCollectionConfig.CollectionName}/points/delete?wait=true",
            body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            ct);
        response.EnsureSuccessStatusCode();
    }
}
