using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class MessagesEndpoints
{
    public static void MapMessagesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/messages").WithTags("Messages");
        group.MapPost("/", SendUserMessage)
            .RequireAuthorization()
            .Produces<MessagePartResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<SendMessageRequest>("application/json");

        group.MapPost("/filter", async ([FromBody] GetMessagesByModel model, IMessagesService messageService, CancellationToken ct) =>
        {
            return Results.Ok(await messageService.GetBatchByAsync(model));
        })

            // .RequireAuthorization()
            .Produces<Message>(StatusCodes.Status200OK)
            .Accepts<GetMessagesByModel>("application/json");
    }

    public static async IAsyncEnumerable<MessagePartResponse> SendUserMessage(
        [FromBody] SendMessageRequest message,
        IMessagesService messagesService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

        if (userId is not null)
        {
            await foreach (var part in messagesService.GenerateAiResponce(message, userId))
            {
                yield return part;
            }
        }
    }
}
