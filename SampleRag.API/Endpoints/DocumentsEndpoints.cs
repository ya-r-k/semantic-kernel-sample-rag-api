using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class DocumentsEndpoints
{
    public static void MapDocumentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/documents").WithTags("Documents");
        group.MapPost("/", async (
            [FromBody] UploadDocumentRequestModel document,
            IDocumentService documentsService,
            CancellationToken ct) =>
        {
            var result = await documentsService.AddAsync(document);
            var created = result.FirstOrDefault();
            return created is not null
                ? Results.Created($"/api/documents/{created.Id}", created)
                : Results.StatusCode(StatusCodes.Status500InternalServerError);
        })
            //.RequireAuthorization("RequireAdministrator")
            .AddEndpointFilter<DocumentUploadValidationFilter>()
            .AddEndpointFilter<FileValidationFilter>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<UploadDocumentRequestModel>("application/json");
    }
}
