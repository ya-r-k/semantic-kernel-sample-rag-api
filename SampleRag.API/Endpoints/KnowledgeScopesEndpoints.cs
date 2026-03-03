using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class KnowledgeScopesEndpoints
{
    public static void MapKnowledgeScopesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/knowledgescopes").WithTags("KnowledgeScopes");

        group.MapPost("/", async (
            [FromBody] CreateGroupRequest request,
            IKnowledgeScopeRepository scopeRepository,
            IKnowledgeScopeUserRepository scopeUserRepository,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";

            var scope = new KnowledgeScope { Name = request.Name };
            var result = await scopeRepository.AddAsync(scope);
            var created = result.FirstOrDefault();
            if (created is not null)
            {
                await scopeUserRepository.AddUserAsync(created.Id, userId, ct);
            }

            return Results.Created($"/api/knowledgescopes/{created?.Id}", created);
        })
            .RequireAuthorization("RequireAdministrator")
            .Produces<KnowledgeScope>(StatusCodes.Status201Created)
            .Accepts<CreateGroupRequest>("application/json");

        group.MapPost("/filter", async (
            GetBatchByModel model,
            IKnowledgeScopeRepository scopeRepository,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

            IEnumerable<KnowledgeScope> result;
            if (!string.IsNullOrEmpty(userId))
            {
                result = await scopeRepository.GetBatchByAsync(model);
            }
            else
            {
                result = await scopeRepository.GetBatchByAsync(model);
            }

            return Results.Ok(result);
        })
            .RequireAuthorization()
            .Produces<IEnumerable<KnowledgeScope>>(StatusCodes.Status200OK);

        group.MapPost("{id:guid}/users", async (
            Guid id,
            [FromBody] AddScopeUserRequest body,
            IKnowledgeScopeUserRepository scopeUserRepository,
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
            IKnowledgeScopeUserRepository scopeUserRepository,
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
