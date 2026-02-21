using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Filters;

public class FileValidationFilter : IEndpointFilter
{
    private const double MaxFileSizeBytes = 1.5 * 1024 * 1024;
    private static readonly string[] AllowedPdfContentTypes = ["application/pdf", "application/x-pdf"];
    private static readonly string[] AllowedPdfExtensions = [".pdf"];

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var document = context.Arguments.OfType<UploadDocumentRequestModel>().FirstOrDefault();
        if (document?.File?.Content is null)
        {
            return Results.ValidationProblem(
            [
                new KeyValuePair<string, string[]>("file.content", ["File content is required"]),
            ]);
        }

        var errors = new Dictionary<string, string[]>();
        var decodedSize = (long)(document.File.Content.Length * 3.0 / 4.0);
        if (decodedSize > MaxFileSizeBytes)
        {
            errors["file.content"] = ["File size exceeds 20 MB limit"];
        }

        var ext = Path.GetExtension(document.File.FileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedPdfExtensions.Contains(ext))
        {
            errors["file.filename"] = ["Only PDF files are allowed"];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        return await next.Invoke(context);
    }
}
