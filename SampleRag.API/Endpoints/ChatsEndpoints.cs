using Microsoft.AspNetCore.Mvc;
using SampleRag.Application.Interfaces;
using SampleRag.Domain.Models;
using System.Linq.Expressions;

namespace SampleRag.API.Endpoints;

public static class ChatsEndpoints
{
    public static void MapChatsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/chats").WithTags("Chats");
        group.MapPost("/", async([FromBody] ChatData chat, IRepository<Guid, ChatData> chatsRepository, CancellationToken ct) => 
        {
            var result = await chatsRepository.AddAsync(chat);

            return Results.Created("api/chats", result);
        })
            .Produces<ChatData>(StatusCodes.Status201Created)
            .Accepts<ChatData>("application/json");

        group.MapGet("/", async ([FromQuery] int batchSize, [FromQuery] Guid? lastUsedIndex, IRepository<Guid, ChatData> chatsRepository, CancellationToken ct) =>
        {
            Expression<Func<ChatData, bool>>? expression = null;

            if (lastUsedIndex.HasValue)
            {
                expression = x => x.Id > lastUsedIndex;
            }

            var result = await chatsRepository.GetBatchByAsync(expression, batchSize);

            return Results.Created("api/chats", result);
        }).Produces<ChatData>(StatusCodes.Status200OK);

        group.MapDelete("{id}", async (Guid id, IRepository<Guid, ChatData> chatsRepository, CancellationToken ct) =>
        {
            await chatsRepository.RemoveByIdsAsync(id);

            return Results.NoContent();
        })
            .Produces<ChatData>(StatusCodes.Status204NoContent)
            .Accepts<ChatData>("application/json");
    }
}
