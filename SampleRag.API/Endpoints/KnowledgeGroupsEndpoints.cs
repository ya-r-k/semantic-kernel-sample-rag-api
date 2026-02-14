using Microsoft.AspNetCore.Mvc;
using SampleRag.Application.Interfaces;
using SampleRag.Domain.Models;

namespace SampleRag.API.Endpoints;

public static class KnowledgeGroupsEndpoints
{
    public static void MapKnowledgeGroupsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/groups").WithTags("Groups");
        group.MapPost("/", async ([FromBody] KnowledgeGroupData knowledgeGroup, IRepository<Guid, KnowledgeGroupData> groupRepository, CancellationToken ct) =>
        {
            var result = await groupRepository.AddAsync(knowledgeGroup);

            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent)
            .Accepts<KnowledgeGroupData>("application/json");

        group.MapGet("/", async (int batchSize, IRepository<Guid, KnowledgeGroupData> groupRepository, CancellationToken ct) =>
        {
            var result = await groupRepository.GetBatchByAsync(null, batchSize);

            return Results.Ok(result);
        }).Produces(StatusCodes.Status200OK);
    }
}
