using Mapster;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Filters;

public class ChatAccessFilter(IChatService chatService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var userId = context.HttpContext.User.Adapt<string>();
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        Guid chatId = Guid.Empty;

        var sendMessage = context.Arguments.OfType<SendMessageRequest>().FirstOrDefault();
        if (sendMessage != null && sendMessage.ChatId.HasValue)
        {
            chatId = sendMessage.ChatId.Value;
        }
        else
        {
            var getMessages = context.Arguments.OfType<GetMessagesByModel>().FirstOrDefault();
            if (getMessages != null && getMessages.ChatId.HasValue)
                chatId = getMessages.ChatId.Value;
        }

        if (chatId == Guid.Empty)
            return await next.Invoke(context);

        var hasAccess = await chatService.HasAccessAsync(chatId, userId, context.HttpContext.RequestAborted);
        if (!hasAccess)
            return Results.Json(new { error = "No access to chat!" }, statusCode: StatusCodes.Status403Forbidden);

        return await next.Invoke(context);
    }
}
