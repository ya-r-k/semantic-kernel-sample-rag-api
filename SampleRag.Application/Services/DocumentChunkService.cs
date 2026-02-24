using System.Linq.Expressions;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SampleRag.Application.Services;

/// <summary>
/// Page-based PDF chunking with optional sub-split for long pages.
/// Uses PdfPig for text extraction.
/// </summary>
public class DocumentChunkService(
    IFileRepository fileRepository,
    IRepository<Guid, DocumentChunk> dbRepository
    ) : IDocumentChunkService
{
    private const int MaxCharsPerChunk = 500;
    private const int OverlapChars = 128;

    /*for (var i = 0; i < result.Count; i++)
        {
            var doc = result[i];
            var fullPath = Path.Combine(storageSettings.BasePath ?? "", "assets/documents", Path.GetFileName(doc.LocalLink));
            if (File.Exists(fullPath))
            {
                var chunks = await chunkingService.ChunkPdfAsync(fullPath, doc.Id, doc.ScopeId);
                await chunkStore.UpsertChunksAsync([.. chunks]);
            }
        }*/

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

            var pageChunks = SplitPageText(text, page);
            foreach (var chunk in pageChunks)
            {
                chunk.ChunkIndex = lastChunkIndex++;
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    public async Task<IEnumerable<DocumentChunk>> GetBatchByAsync(Expression<Func<DocumentChunk, bool>> predicate, int batchSize)
    {
        return await dbRepository.GetBatchByAsync(predicate, batchSize);
        throw new NotImplementedException();
    }

    private static List<DocumentChunk> SplitPageText(string text, int pageNumber)
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
