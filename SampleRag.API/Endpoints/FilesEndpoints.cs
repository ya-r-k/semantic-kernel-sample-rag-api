using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Interfaces;

namespace SampleRag.API.Endpoints;

public static class FilesEndpoints
{
    public static void MapFilesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/files")
            .WithTags("Files")
            .RequireAuthorization();

        group.MapGet("assets/documents/{scopeId:guid}/{fileName}", async ([FromRoute] Guid scopeId, [FromRoute] string fileName, IFileRepository repository, CancellationToken ct) =>
        {
            var stream = await repository.GetAsync($"assets/documents/{scopeId}", fileName);
            if (stream is null)
            {
                return Results.NotFound();
            }

            return Results.File(stream, enableRangeProcessing: true, contentType: "application/pdf");
        })
            .AddEndpointFilter<RouteScopeAccessFilter>()
            .Produces<FileStreamResult>(StatusCodes.Status200OK)
            .Produces<FileStreamResult>(StatusCodes.Status206PartialContent)
            .Produces<FileStreamResult>(StatusCodes.Status401Unauthorized)
            .Produces<NotFound>(StatusCodes.Status404NotFound);
    }
}
