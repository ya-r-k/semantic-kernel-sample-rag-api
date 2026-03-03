using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class ChatsEndpoints
{
    public static void MapChatsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/chats").WithTags("Chats");
        group.MapPost("/", async ([FromBody] CreateChatRequest request, IChatService chatService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";
            var ownerIds = request.OwnerIds?.Length > 0 ? request.OwnerIds : [userId];

            var chat = new Chat
            {
                Name = request.Title,
                ScopeId = request.ScopeId,
                OwnerIds = ownerIds,
            };

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

        group.MapGet("/", async ([FromQuery] int batchSize, [FromQuery] Guid? lastUsedIndex, IRepository<Guid, Chat> chatsRepository, CancellationToken ct) =>
        {
            Expression<Func<Chat, bool>>? expression = null;

            if (lastUsedIndex.HasValue)
            {
                expression = x => x.Id > lastUsedIndex;
            }

            var result = await chatsRepository.GetBatchByAsync(expression, batchSize);

            return Results.Ok(result);
        })
            .RequireAuthorization()
            .Produces<Chat>(StatusCodes.Status200OK);

        group.MapDelete("{id}", async (Guid id, IRepository<Guid, Chat> chatsRepository, CancellationToken ct) =>
        {
            await chatsRepository.RemoveByIdsAsync(id);

            return Results.NoContent();
        })
            .RequireAuthorization()
            .Produces<Chat>(StatusCodes.Status204NoContent)
            .Accepts<Chat>("application/json");
    }
}
