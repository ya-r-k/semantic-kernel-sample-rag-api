using Microsoft.AspNetCore.Mvc;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class DocumentsEndpoints
{
    public static void MapDocumentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/documents").WithTags("Documents");
        group.MapPost("/", async ([FromBody] UploadDocumentRequestModel document, IDocumentService documentsService, CancellationToken ct) =>
        {
            var result = await documentsService.AddAsync(document);

            return Results.NoContent();
        })
            .RequireAuthorization("RequireAdministrator")
            .Produces(StatusCodes.Status204NoContent)
            .Accepts<UploadDocumentRequestModel>("application/json");
    }
}
