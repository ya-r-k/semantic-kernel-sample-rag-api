using SampleRag.Application.Interfaces.Services;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SampleRag.Application.Services;

/// <summary>
/// Page-based PDF chunking with optional sub-split for long pages.
/// Uses PdfPig for text extraction.
/// </summary>
public class ChunkingService : IChunkingService
{
    private const int MaxCharsPerChunk = 2000;
    private const int OverlapChars = 128;

    public async Task<IReadOnlyList<DocumentChunkInfo>> ChunkPdfAsync(string pdfPath, Guid documentId, Guid scopeId, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            var chunks = new List<DocumentChunkInfo>();
            using var document = PdfDocument.Open(pdfPath);

            for (var pageNum = 1; pageNum <= document.NumberOfPages; pageNum++)
            {
                ct.ThrowIfCancellationRequested();
                var page = document.GetPage(pageNum);
                var text = ContentOrderTextExtractor.GetText(page);
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var pageChunks = SplitPageText(text, pageNum);
                foreach (var (chunkText, chunkIndex) in pageChunks)
                {
                    chunks.Add(new DocumentChunkInfo(documentId, scopeId, pageNum, chunkIndex, chunkText));
                }
            }

            return chunks;
        }, ct);
    }

    private static List<(string Text, int? ChunkIndex)> SplitPageText(string text, int pageNumber)
    {
        if (text.Length <= MaxCharsPerChunk)
            return [(text, null)];

        var chunks = new List<(string Text, int? ChunkIndex)>();
        var start = 0;
        var chunkIndex = 0;

        while (start < text.Length)
        {
            var length = Math.Min(MaxCharsPerChunk, text.Length - start);
            var chunkText = text.Substring(start, length);
            chunks.Add((chunkText, chunkIndex));
            chunkIndex++;
            start += length - (start + length < text.Length ? OverlapChars : 0);
        }

        return chunks;
    }
}
