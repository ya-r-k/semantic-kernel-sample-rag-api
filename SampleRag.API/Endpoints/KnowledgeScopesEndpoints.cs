using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.RequestModels;

namespace SampleRag.API.Endpoints;

public static class KnowledgeScopesEndpoints
{
    public static void MapKnowledgeScopesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/knowledgescopes")
            .WithTags("KnowledgeScopes")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateScopeRequest[] request,
            IKnowledgeScopeService scopeService,
            ClaimsPrincipal claims,
            CancellationToken ct) =>
        {
            //var scopes = (request, claims).Adapt<CreateScopeRequest[]>();
            var scopes = request.Adapt<CreateScopeRequest[]>();

            var result = await scopeService.AddAsync(scopes);
            return Results.Created($"/api/knowledgescopes/", result);
        })
            .RequireAuthorization("RequireAdmin")
            .Produces<KnowledgeScope>(StatusCodes.Status201Created)
            .Accepts<CreateScopeRequest>("application/json");

        group.MapPost("/filter", async (
            [FromBody] GetBatchByModel model,
            ClaimsPrincipal claims,
            IKnowledgeScopeService scopeService,
            CancellationToken ct) =>
        {
            return Results.Ok(await scopeService.GetBatchByAsync(model, ct));
        })
            .Produces<IEnumerable<KnowledgeScope>>(StatusCodes.Status200OK);

        group.MapPost("{id:guid}/users", async (
            Guid id,
            [FromBody] AddScopeUserRequest body,
            IKnowledgeScopeService scopeUserService,
            CancellationToken ct) =>
        {
            await scopeUserService.AddUsersAsync(id, body.UsersId, ct);
            return Results.NoContent();
        })
            .RequireAuthorization("RequireAdmin")
            .Produces(StatusCodes.Status204NoContent)
            .Accepts<AddScopeUserRequest>("application/json");

        group.MapDelete("{id:guid}/users/{userId}", async (
            Guid id,
            string userId,
            IKnowledgeScopeUserRepository scopeUserRepository,
            CancellationToken ct) =>
        {
            await scopeUserRepository.RemoveUserAsync(id, [userId], ct);
            return Results.NoContent();
        })
            .RequireAuthorization("RequireAdmin")
            .Produces(StatusCodes.Status204NoContent);
    }
}
