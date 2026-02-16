using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Application.Interfaces;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class DocumentsEndpoints
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB
    private static readonly string[] AllowedPdfContentTypes = ["application/pdf", "application/x-pdf"];
    private static readonly string[] AllowedPdfExtensions = [".pdf"];

    public static void MapDocumentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/documents").WithTags("Documents");
        group.MapPost("/", async (
            [FromBody] UploadDocumentRequestModel document,
            IDocumentService documentsService,
            IScopeAccessService scopeAccessService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if (!await scopeAccessService.CanUseScopeAsync(document.ScopeId, userId, ct))
                return Results.Json(new { error = "No access to scope" }, statusCode: StatusCodes.Status403Forbidden);

            var decodedSize = document.File?.Data != null
                ? (long)(document.File.Data.Length * 3.0 / 4.0)
                : 0;
            if (decodedSize > MaxFileSizeBytes)
                return Results.Json(new { error = "File size exceeds 20 MB limit" }, statusCode: StatusCodes.Status400BadRequest);

            var ext = Path.GetExtension(document.File?.FileName ?? "").ToLowerInvariant();
            var contentType = document.File?.ContentType?.ToLowerInvariant() ?? "";
            var isPdf = AllowedPdfExtensions.Contains(ext) || AllowedPdfContentTypes.Contains(contentType);
            if (!isPdf)
                return Results.Json(new { error = "Only PDF files are allowed" }, statusCode: StatusCodes.Status400BadRequest);

            var result = await documentsService.AddAsync(document);
            var created = result.FirstOrDefault();
            return created is not null
                ? Results.Created($"/api/documents/{created.Id}", created)
                : Results.StatusCode(StatusCodes.Status500InternalServerError);
        })
            .RequireAuthorization("RequireAdministrator")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<UploadDocumentRequestModel>("application/json");
    }
}
