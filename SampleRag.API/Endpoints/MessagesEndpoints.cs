using Microsoft.AspNetCore.Mvc;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Domain.Models;
using System.Runtime.CompilerServices;

namespace SampleRag.API.Endpoints;

public static class MessagesEndpoints
{
    public static void MapMessagesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/messages").WithTags("Messages");
        group.MapPost("/", SendUserMessage)
            .RequireAuthorization()
            .Produces<MessageData>(StatusCodes.Status200OK)
            .Accepts<MessageData>("application/json");
    }

    public static async IAsyncEnumerable<MessagePart> SendUserMessage(
        [FromBody] MessageData message,
        IMessageService<Guid> messagesService)
    {
        await foreach (var part in messagesService.GenerateAiResponce(message))
        {
            yield return part;
        }
    }
}
