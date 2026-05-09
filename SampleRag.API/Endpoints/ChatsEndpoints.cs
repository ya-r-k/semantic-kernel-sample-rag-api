using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class ChatsEndpoints
{
    public static void MapChatsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/chats")
            .WithTags("Chats")
            .RequireAuthorization();

        group.MapPost("/", async ([FromBody] CreateChatRequest request, IChatService chatService, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var chat = (request, claims).Adapt<Chat>();

            var result = await chatService.AddAsync(chat);
            var created = result.FirstOrDefault();

            return created is not null
                ? Results.Created($"/api/chats/{created.Id}", created)
                : Results.StatusCode(StatusCodes.Status500InternalServerError);
        })
            .AddEndpointFilter<BodyScopeAccessFilter>()
            .Produces<Chat>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<CreateChatRequest>("application/json");

        group.MapPost("/filter", async ([FromBody] GetChatsByModel model, IChatService chatService, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var userId = claims.Adapt<string>();
            var chats = await chatService.GetBatchByAsync(model, userId, ct);

            return Results.Ok(chats);
        })
            .Produces<Chat>(StatusCodes.Status200OK);
    }
}
