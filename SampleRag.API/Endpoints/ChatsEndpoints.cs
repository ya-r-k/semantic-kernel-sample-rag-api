using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using SampleRag.Application.Interfaces;
using SampleRag.Domain.Models;
using System.Linq.Expressions;

namespace SampleRag.API.Endpoints;

public static class ChatsEndpoints
{
    public static void MapChatsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/chats").WithTags("Chats");
        group.MapPost("/", async([FromBody] ChatData chat, IRepository<int, ChatData> chatsRepository, Kernel kernel, CancellationToken ct) => 
        {
            var result = await chatsRepository.AddAsync(chat);

            return Results.Created("api/chats", result);
        })
            .Produces<ChatData>(StatusCodes.Status201Created)
            .Accepts<ChatData>("application/json");

        group.MapGet("/", async ([FromQuery] int batchSize, [FromQuery] int? lastUsedIndex, IRepository<int, ChatData> chatsRepository, Kernel kernel, CancellationToken ct) =>
        {
            Expression<Func<ChatData, bool>>? expression = null;

            if (lastUsedIndex.HasValue)
            {
                expression = x => x.Id > lastUsedIndex;
            }

            var result = await chatsRepository.GetBatchByAsync(expression, batchSize);

            return Results.Created("api/chats", result);
        })
            .Produces<ChatData>(StatusCodes.Status200OK)
            .Accepts<ChatData>("application/json");

        group.MapDelete("{id}", async (int id, IRepository<int, ChatData> chatsRepository, Kernel kernel, CancellationToken ct) =>
        {
            await chatsRepository.RemoveByIdsAsync(id);

            return Results.NoContent();
        })
            .Produces<ChatData>(StatusCodes.Status204NoContent)
            .Accepts<ChatData>("application/json");
    }
}
