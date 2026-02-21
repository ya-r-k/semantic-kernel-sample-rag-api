using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Filters;

public class DocumentUploadValidationFilter : IEndpointFilter
{
    private const int MaxNameLength = 500;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var document = context.Arguments.OfType<UploadDocumentRequestModel>().FirstOrDefault();
        if (document is null)
        {
            return Results.BadRequest("Request body with document data is required");
        }

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(document.Name))
        {
            errors["name"] = ["Document name is required"];
        }
        else if (document.Name.Length > MaxNameLength)
        {
            errors["name"] = [$"Document name cannot exceed {MaxNameLength} characters"];
        }

        if (document.ScopeId == Guid.Empty)
        {
            errors["scopeId"] = ["Scope ID is required"];
        }

        if (document.File is null)
        {
            errors["file"] = ["File data is required"];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        return await next.Invoke(context);
    }
}
