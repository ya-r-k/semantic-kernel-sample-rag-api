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
        var group = routes.MapGroup("api/chats").WithTags("Chats");
        group.MapPost("/", async ([FromBody] CreateChatRequest request, IChatService chatService, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var chat = (request, claims).Adapt<Chat>();

            var result = await chatService.AddAsync(chat);
            var created = result.FirstOrDefault();

            return created is not null
                ? Results.Created($"/api/chats/{created.Id}", created)
                : Results.StatusCode(StatusCodes.Status500InternalServerError);
        })
            .RequireAuthorization()
            .AddEndpointFilter<ScopeUserAccessFilter>()
            .Produces<Chat>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<CreateChatRequest>("application/json");

        group.MapPost("/filter", async ([FromBody] GetChatsByModel model, IChatService chatService, CancellationToken ct) =>
        {
            return Results.Ok(await chatService.GetBatchByAsync(model));
        })
            .RequireAuthorization()
            .Produces<Chat>(StatusCodes.Status200OK);

        group.MapPost("{id:guid}/owners", async (Guid id, [FromBody] AddChatOwnerRequest request, IChatService chatService, ClaimsPrincipal claims, CancellationToken ct) =>
        {
            var callerUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? claims.FindFirstValue("sub");
            if (string.IsNullOrEmpty(callerUserId))
            {
                return Results.Unauthorized();
            }

            var chats = await chatService.GetByIdsAsync(id);
            var chat = chats.FirstOrDefault();
            if (chat is null)
            {
                return Results.NotFound();
            }

            if (chat.OwnerIds is null || !chat.OwnerIds.Contains(callerUserId))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return Results.BadRequest("UserId is required.");
            }

            if (chat.OwnerIds.Contains(request.UserId))
            {
                return Results.NoContent();
            }

            chat.OwnerIds = [.. chat.OwnerIds, request.UserId];
            await chatService.UpdateAsync(chat);

            return Results.NoContent();
        })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Accepts<AddChatOwnerRequest>("application/json");

        group.MapPatch("{id:guid}/name/generate", async (Guid id, IChatService chatService, CancellationToken ct) =>
        {
            throw new NotImplementedException();
        })
            .RequireAuthorization()
            .Produces<Chat>(StatusCodes.Status204NoContent);

        group.MapDelete("{id:guid}", async (Guid id, IChatService chatService, CancellationToken ct) =>
        {
            await chatService.RemoveByIdsAsync(id);

            return Results.NoContent();
        })
            .RequireAuthorization()
            .Produces<Chat>(StatusCodes.Status204NoContent)
            .Accepts<Chat>("application/json");
    }
}
