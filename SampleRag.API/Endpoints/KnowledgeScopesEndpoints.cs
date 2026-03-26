using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Enums;
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
            CancellationToken ct) =>
        {
            var result = await scopeService.AddAsync(request, ct);

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
            var role = claims.Adapt<UserRole>();
            return Results.Ok(await scopeService.GetBatchByAsync(model, role, ct));
        })
            .Produces<IEnumerable<KnowledgeScope>>(StatusCodes.Status200OK)
            .Accepts<GetBatchByModel>("application/json");

        group.MapPatch("{id:guid}/roles", async (
            Guid id,
            [FromBody] UpdateScopeRolesRequest body,
            IKnowledgeScopeService scopeService,
            CancellationToken ct) =>
        {
            await scopeService.UpdateRolesAsync(id, body.AddingRoles, body.RemovingRoles, ct);

            return Results.NoContent();
        })
            .RequireAuthorization("RequireAdmin")
            .Produces(StatusCodes.Status204NoContent)
            .Accepts<UpdateScopeRolesRequest>("application/json");
    }
}
