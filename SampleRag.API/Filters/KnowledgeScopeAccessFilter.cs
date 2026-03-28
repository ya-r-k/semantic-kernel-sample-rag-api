using System.Security.Claims;
using Mapster;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Abstractions;
using SampleRag.Domain.Models.Enums;

namespace SampleRag.API.Filters;

public class KnowledgeScopeAccessFilter(
    IKnowledgeScopeService scopeAccessService,
    ClaimsPrincipal claims) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var data = context.Arguments.OfType<IEntityWithScopeId>().FirstOrDefault();
        if (data is null)
        {
            return Results.BadRequest("Entity with ScopeId required");
        }

        var role = claims.Adapt<UserRole>();
        if (!await scopeAccessService.HasAccessAsync(data.ScopeId, role))
        {
            return Results.Json(new { error = "No access to scope" }, statusCode: StatusCodes.Status403Forbidden);
        }

        return await next.Invoke(context);
    }
}
