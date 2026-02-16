using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Application.Interfaces;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class KnowledgeGroupsEndpoints
{
    public static void MapKnowledgeGroupsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/groups").WithTags("Groups");

        group.MapPost("/", async (
            [FromBody] CreateGroupRequest request,
            IRepository<Guid, KnowledgeGroupData> groupRepository,
            IScopeUserRepository scopeUserRepository,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var scope = new KnowledgeGroupData { Name = request.Name };
            var result = await groupRepository.AddAsync(scope);
            var created = result.FirstOrDefault();
            if (created is not null)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";
                await scopeUserRepository.AddUserAsync(created.Id, userId, ct);
            }
            return Results.Created($"/api/groups/{created?.Id}", created);
        })
            .RequireAuthorization("RequireAdministrator")
            .Produces<KnowledgeGroupData>(StatusCodes.Status201Created)
            .Accepts<CreateGroupRequest>("application/json");

        group.MapGet("/", async (
            [FromQuery] int batchSize,
            IRepository<Guid, KnowledgeGroupData> groupRepository,
            IScopeUserRepository scopeUserRepository,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            IEnumerable<KnowledgeGroupData> result;
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
            .Produces<IEnumerable<KnowledgeGroupData>>(StatusCodes.Status200OK);

        group.MapPost("{id:guid}/users", async (
            Guid id,
            [FromBody] AddScopeUserRequest body,
            IScopeUserRepository scopeUserRepository,
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
            IScopeUserRepository scopeUserRepository,
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
