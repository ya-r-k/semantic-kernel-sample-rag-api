using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Filters;

public class BulkDocumentUploadValidationFilter : IEndpointFilter
{
    private const int MaxNameLength = 500;
    private const double MaxFileSizeBytes = 20 * 1024 * 1024;
    private static readonly string[] AllowedPdfContentTypes = ["application/pdf", "application/x-pdf"];
    private static readonly string[] AllowedPdfExtensions = [".pdf"];

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var documents = context.Arguments.OfType<UploadDocumentRequestModel[]>().FirstOrDefault();
        if (documents is null || documents.Length > 0)
        {
            return Results.BadRequest("Request body with documents data is required");
        }

        var errors = new Dictionary<string, string[]>();
        if (documents.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            errors["name"] = ["Document name is required"];
        }
        else if (documents.Any(x => x.Name.Length > MaxNameLength))
        {
            errors["name"] = [$"Document name cannot exceed {MaxNameLength} characters"];
        }

        if (documents.Any(x => x.ScopeId == Guid.Empty))
        {
            errors["scopeId"] = ["Scope ID is required"];
        }

        if (documents.Any(x => x.File is null))
        {
            errors["file"] = ["File data is required"];
        }
        else if (documents.Any(x => x.File?.Content is null))
        {
            errors["file.content"] = ["File content is required"];
        }
        else
        {
            if (documents.Any(x => x.File.Content.Length * 3.0 / 4.0 > MaxFileSizeBytes))
            {
                errors["file.content"] = ["File size exceeds 20 MB limit"];
            }

            if (documents.Any(x => !AllowedPdfExtensions.Contains(
                Path.GetExtension(x.File.FileName ?? string.Empty).ToLowerInvariant())))
            {
                errors["file.filename"] = ["Only PDF files are allowed"];
            }
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        return await next.Invoke(context);
    }
}
