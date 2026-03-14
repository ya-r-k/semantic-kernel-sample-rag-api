using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class FeedbacksEndpoints
{
    public static void MapFeedbacksEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/feedbacks").WithTags("Feedbacks");

        group.MapPost("/", async ([FromBody] FeedbackRequest request, ClaimsPrincipal claims, IFeedbackService feedbackService, CancellationToken ct) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? claims.FindFirstValue("sub");
            await feedbackService.UpsertFeedbackAsync(request.Adapt<Feedback>(), ct);

            return Results.NoContent();
        })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Accepts<FeedbackRequest>("application/json");

        group.MapPost("/filter", async ([FromBody] GetFeedbackByModel filterModel, IFeedbackService feedbackService, CancellationToken ct) =>
        {
            return Results.Ok(await feedbackService.GetFeedbackAsync(filterModel, ct));
        })
            .RequireAuthorization()
            .Produces<IEnumerable<Feedback>>(StatusCodes.Status200OK);
    }
}
