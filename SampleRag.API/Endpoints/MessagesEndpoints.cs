using Microsoft.AspNetCore.Mvc;
using Quartz;
using SampleRag.Domain.Models;

namespace SampleRag.API.Endpoints;

public static class MessagesEndpoints
{
    public static void MapDataChunkEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/messages").WithTags("Messages");
        group.MapPost("/", async ([FromBody] MessageData message, ISchedulerFactory schedulerFactory) =>
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var jobDataMap = new JobDataMap
            {
                ["prompt"] = message.Text,
            };

            if (message.ConversationId.HasValue)
            {
                jobDataMap.Add("conversationId", message.ConversationId.Value);
            }

            await scheduler.TriggerJob(new JobKey("RagResponseGeneration"), jobDataMap);

            return Results.NoContent();
        });

        group.MapPost("/ids", async ([FromBody] Guid[] ids, IService<DataChunk> service) =>
        {
            var chunks = await service.GetByIdsAsync(ids);

            return chunks is not null ? Results.Ok(chunks) : Results.NotFound();
        });

        group.MapDelete("/ids", async ([FromBody] Guid[] ids, IService<DataChunk> service) =>
        {
            await service.RemoveByIdsAsync(ids);

            return Results.NoContent();
        });
    }
}
