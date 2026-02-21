using Microsoft.AspNetCore.Mvc;
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
            .Produces<MessagePart>(StatusCodes.Status200OK)
            .Accepts<SendMessageRequest>("application/json");
    }

    public static async IAsyncEnumerable<MessagePart> SendUserMessage(
        [FromBody] SendMessageRequest message,
        IMessagesService messagesService)
    {
        await foreach (var part in messagesService.GenerateAiResponce(message))
        {
            yield return part;
        }
    }
}
