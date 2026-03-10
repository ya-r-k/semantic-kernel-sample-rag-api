using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Entities.Db;
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

        group.MapPost("/filter", async ([FromBody] GetDocumentsByModel model, IDocumentService documentService, CancellationToken ct) =>
        {
            return Results.Ok(await documentService.GetBatchByAsync(model));
        })
            //.RequireAuthorization("RequireAdministrator")
            .Produces<Document>(StatusCodes.Status200OK)
            .Accepts<GetDocumentsByModel>("application/json");

        group.MapDelete("{id:guid}", async (Guid id, IDocumentService documentService, CancellationToken ct) =>
        {
            await documentService.RemoveByIdsAsync(id);

            return Results.NoContent();
        })
            //.RequireAuthorization("RequireAdministrator")
            .Produces<Document>(StatusCodes.Status204NoContent);
    }
}
