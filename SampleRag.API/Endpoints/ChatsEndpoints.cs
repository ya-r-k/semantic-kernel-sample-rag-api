using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Interfaces;
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
