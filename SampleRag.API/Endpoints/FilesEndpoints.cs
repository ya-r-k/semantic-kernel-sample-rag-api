using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Application.Interfaces;

namespace SampleRag.API.Endpoints;

public static class FilesEndpoints
{
    public static void MapFilesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/files").WithTags("Files");
        group.MapGet("assets/documents/{fileName}", async ([FromRoute] string fileName, IFileRepository repository, CancellationToken ct) =>
        {
            await using var stream = await repository.GetAsync("assets/documents", fileName);

            return Results.File(stream, fileDownloadName: fileName, enableRangeProcessing: true);
        })
            //.RequireAuthorization()
            .Produces<FileStreamResult>(StatusCodes.Status200OK)
            .Produces<FileStreamResult>(StatusCodes.Status206PartialContent)
            .Produces<FileStreamResult>(StatusCodes.Status401Unauthorized)
            .Produces<NotFound>(StatusCodes.Status404NotFound);
    }
}
