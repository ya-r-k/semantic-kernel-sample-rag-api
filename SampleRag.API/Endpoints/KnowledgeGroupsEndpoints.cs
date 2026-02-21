using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class KnowledgeGroupsEndpoints
{
    public static void MapKnowledgeScopesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/knowledgescopes").WithTags("KnowledgeScopes");

        group.MapPost("/", async (
            [FromBody] CreateGroupRequest request,
            IRepository<Guid, KnowledgeScope> groupRepository,
            IKnowledgeGroupUserRepository scopeUserRepository,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var scope = new KnowledgeScope { Name = request.Name };
            var result = await groupRepository.AddAsync(scope);
            var created = result.FirstOrDefault();
            if (created is not null)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";
                await scopeUserRepository.AddUserAsync(created.Id, userId, ct);
            }
            return Results.Created($"/api/knowledgescopes/{created?.Id}", created);
        })
            .RequireAuthorization("RequireAdministrator")
            .Produces<KnowledgeScope>(StatusCodes.Status201Created)
            .Accepts<CreateGroupRequest>("application/json");

        group.MapGet("/", async (
            [FromQuery] int batchSize,
            IRepository<Guid, KnowledgeScope> groupRepository,
            IKnowledgeGroupUserRepository scopeUserRepository,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            IEnumerable<KnowledgeScope> result;
            if (!string.IsNullOrEmpty(userId))
            {
                var scopeIds = await scopeUserRepository.GetScopeIdsForUserAsync(userId, ct);
                var allScopes = await groupRepository.GetBatchByAsync(null, batchSize);
                result = allScopes.Where(s => scopeIds.Contains(s.Id));
            }
            else
            {
                result = await groupRepository.GetBatchByAsync(null, batchSize);
            }
            return Results.Ok(result);
        })
            .RequireAuthorization()
            .Produces<IEnumerable<KnowledgeScope>>(StatusCodes.Status200OK);

        group.MapPost("{id:guid}/users", async (
            Guid id,
            [FromBody] AddScopeUserRequest body,
            IKnowledgeGroupUserRepository scopeUserRepository,
            CancellationToken ct) =>
        {
            await scopeUserRepository.AddUserAsync(id, body.UserId, ct);
            return Results.NoContent();
        })
            .RequireAuthorization("RequireAdministrator")
            .Produces(StatusCodes.Status204NoContent)
            .Accepts<AddScopeUserRequest>("application/json");

        group.MapDelete("{id:guid}/users/{userId}", async (
            Guid id,
            string userId,
            IKnowledgeGroupUserRepository scopeUserRepository,
            CancellationToken ct) =>
        {
            await scopeUserRepository.RemoveUserAsync(id, userId, ct);
            return Results.NoContent();
        })
            .RequireAuthorization("RequireAdministrator")
            .Produces(StatusCodes.Status204NoContent);
    }
}

public record AddScopeUserRequest(string UserId);
