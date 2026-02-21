using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;
using System.Linq.Expressions;

namespace SampleRag.API.Endpoints;

public static class ChatsEndpoints
{
    public static void MapChatsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/chats").WithTags("Chats");
        group.MapPost("/", async ([FromBody] Chat chat, IRepository<Guid, Chat> chatsRepository, CancellationToken ct) =>
        {
            var result = await chatsRepository.AddAsync(chat);

            return Results.Created("api/chats", result);
        })
            .RequireAuthorization()
            .Produces<Chat>(StatusCodes.Status201Created)
            .Accepts<Chat>("application/json");

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
