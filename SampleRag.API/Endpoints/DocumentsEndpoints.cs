using Mapster;
using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class DocumentsEndpoints
{
    public static void MapDocumentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/documents")
            .WithTags("Documents")
            .RequireAuthorization("RequireAdmin");

        group.MapPost("/", async (
            [FromBody] UploadDocumentRequestModel document,
            IDocumentService documentsService,
            CancellationToken ct) =>
        {
            var created = await documentsService.AddAsync(document);
            return created is not null
                ? Results.Created($"/api/documents/filter/ids", created)
                : Results.StatusCode(StatusCodes.Status500InternalServerError);
        })
            .AddEndpointFilter<DocumentUploadValidationFilter>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<UploadDocumentRequestModel>("application/json");

        group.MapPost("/bulk", async (
            [FromBody] UploadDocumentRequestModel[] documents,
            IDocumentService documentsService,
            CancellationToken ct) =>
        {
            var created = await documentsService.AddAsync(documents);
            return created is not null && created.Any()
                ? Results.Created($"/api/documents/filter/ids", created)
                : Results.StatusCode(StatusCodes.Status500InternalServerError);
        })
            .RequireAuthorization("RequireSuperAdmin")
            .AddEndpointFilter<BulkDocumentUploadValidationFilter>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<UploadDocumentRequestModel>("application/json");

        group.MapPut("/", async (
            [FromBody] UpdateDocumentRequestModel request,
            IDocumentService documentsService,
            CancellationToken ct) =>
        {
            await documentsService.UpdateAsync(
                [request.Adapt<Document>()],
                [nameof(Document.Id), nameof(Document.Name), nameof(Document.ScopeId), nameof(Document.OriginalLink)]);

            return Results.NoContent();
        })
            .AddEndpointFilter<BodyScopeAccessFilter>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<UploadDocumentRequestModel>("application/json");

        group.MapPut("/outdated/{id:guid}", async (Guid id, [FromBody] UpdateDocumentOutdatedRequestModel request, IDocumentService documentService, CancellationToken ct) =>
        {
            var document = new Document
            {
                Id = id,
                IsOutOfDate = request.IsOutOfDate
            };

            await documentService.UpdateAsync(
                new[] { document },
                new[] { nameof(Document.IsOutOfDate) });

            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Accepts<UpdateDocumentOutdatedRequestModel>("application/json");

        group.MapPost("/filter", async ([FromBody] GetDocumentsByModel model, IDocumentService documentService, CancellationToken ct) =>
        {
            return Results.Ok(await documentService.GetBatchByAsync(model));
        })
            .Produces<Document>(StatusCodes.Status200OK)
            .Accepts<GetDocumentsByModel>("application/json");

        group.MapPost("/filter/ids", async ([FromBody] Guid[] ids, IDocumentService documentService, CancellationToken ct) =>
        {
            return Results.Ok(await documentService.GetByIdsAsync(ids));
        })
            .Produces<Document>(StatusCodes.Status200OK)
            .Accepts<Guid[]>("application/json");

        group.MapDelete("{id:guid}", async (Guid id, IDocumentService documentService, CancellationToken ct) =>
        {
            await documentService.RemoveByIdsAsync(id);

            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/chunks", async (IDocumentService documentService, CancellationToken ct) =>
        {
            await documentService.RemoveAllChunksAsync(ct);

            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/chunks/embeddings", async (IDocumentChunkService documentChunkService, CancellationToken ct) =>
        {
            await documentChunkService.RemoveAllEmbeddingsAsync(ct);

            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent);
    }
}
