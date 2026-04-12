using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SampleRag.API.Filters;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class MessagesEndpoints
{
    public static void MapMessagesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/messages")
            .WithTags("Messages");
        //.RequireAuthorization();

        group.MapPost("/complexity", async ([FromBody] string message, ITextAnalyzer textAnalyzer) =>
        {
            return Results.Ok(await textAnalyzer.DetermineQueryComplexity(message));
        });

        group.MapPost("/language", async ([FromBody] string message, ITextAnalyzer textAnalyzer) =>
        {
            return Results.Ok(await textAnalyzer.DetectLanguageAsync(message));
        });

        group.MapPost("/", ([FromBody] SendMessageRequest message, IMessagesService messagesService, ClaimsPrincipal user) =>
        {
            var userId = user.Adapt<string>();

            return Results.ServerSentEvents(messagesService.GenerateAiResponce(message, userId));
        })
            .RequireRateLimiting("send-message")
            .AddEndpointFilter<BodyScopeAccessFilter>()
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
