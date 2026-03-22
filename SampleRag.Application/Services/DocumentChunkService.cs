using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SampleRag.Application.Services;

/// <summary>
/// Page-based PDF chunking with optional sub-split for long pages.
/// Uses PdfPig for text extraction.
/// </summary>
public class DocumentChunkService(
    IFileRepository fileRepository,
    IFilterRepository<Guid, DocumentChunk, GetDocumentChunksByModel> dbRepository,
    IVectorRepository<DocumentChunk> vectorRepository) : IDocumentChunkService
{
    private const int MaxCharsPerChunk = 512;
    private const int OverlapChars = 128;

    public async Task<IEnumerable<DocumentChunk>> ChunkAsync(Document data, CancellationToken ct = default)
    {
        await using var stream = await fileRepository.GetAsync(string.Empty, data.LocalLink);

        if (stream is null)
        {
            return [];
        }

        var chunks = new List<DocumentChunk>();
        using var document = PdfDocument.Open(stream);

        var lastChunkIndex = 0;

        for (var page = 1; page <= document.NumberOfPages; page++)
        {
            ct.ThrowIfCancellationRequested();

            var pageContent = document.GetPage(page);
            var text = ContentOrderTextExtractor.GetText(pageContent);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var pageChunks = this.SplitPageText(text, page);
            foreach (var chunk in pageChunks)
            {
                chunk.DocumentId = data.Id;
                chunk.ScopeId = data.ScopeId;
                chunk.ChunkIndex = lastChunkIndex++;

                chunks.Add(chunk);
            }
        }

        return await dbRepository.AddAsync([.. chunks]);
    }

    public async Task<IEnumerable<DocumentChunk>> GetBatchByAsync(GetDocumentChunksByModel model)
    {
        return await dbRepository.GetBatchByAsync(model);
    }

    public async Task RemoveAllAsync(CancellationToken ct = default)
    {
        await dbRepository.ClearAsync(ct);
        await vectorRepository.ClearAsync(ct);
    }

    public async Task RemoveAllEmbeddingsAsync(CancellationToken ct = default)
    {
        await dbRepository.SetFieldValueAsync(x => x.IsVectorized, false);
        await vectorRepository.ClearAsync(ct);
    }

    public async Task<IEnumerable<DocumentChunk>> RetrieveChunksAsync(string query, int topK = 5, CancellationToken ct = default)
    {
        var chunks = await vectorRepository.RetrieveChunksAsync(query, topK, ct);

        return await dbRepository.GetByIdsAsync([.. chunks.Select(x => x.Id)]);
    }

    private List<DocumentChunk> SplitPageText(string text, int pageNumber)
    {
        if (text.Length <= MaxCharsPerChunk)
        {
            return
            [
                new DocumentChunk
                {
                    PageNumber = pageNumber,
                    Text = text,
                },
            ];
        }

        var chunks = new List<DocumentChunk>();
        var start = 0;

        while (start < text.Length)
        {
            var length = Math.Min(MaxCharsPerChunk, text.Length - start);
            var chunkText = text.Substring(start, length);
            chunks.Add(new DocumentChunk
            {
                Text = chunkText,
                PageNumber = pageNumber,
            });
            start += length - (start + length < text.Length ? OverlapChars : 0);
        }

        return chunks;
    }
}
