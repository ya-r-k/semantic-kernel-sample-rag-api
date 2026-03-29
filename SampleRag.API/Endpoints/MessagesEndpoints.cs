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
        var group = routes.MapGroup("api/messages")
            .WithTags("Messages")
            .RequireAuthorization();

        group.MapPost("/", ([FromBody] SendMessageRequest message, IMessagesService messagesService, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            var role = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("roles");

            return Results.ServerSentEvents(messagesService.GenerateAiResponce(message, role, userId));
        })
            .RequireRateLimiting("send-message")
            .Produces<MessagePartResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Accepts<SendMessageRequest>("application/json");

        group.MapPost("/filter", async ([FromBody] GetMessagesByModel model, IMessagesService messageService, CancellationToken ct) =>
        {
            return Results.Ok(await messageService.GetBatchByAsync(model));
        })
            .Produces<Message>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Accepts<GetMessagesByModel>("application/json");
    }
}
