using System.Security.Claims;
using Mapster;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Abstractions;
using SampleRag.Domain.Models.Enums;

namespace SampleRag.API.Filters;

public class ScopeUserAccessFilter(
    IKnowledgeScopeService scopeAccessService,
    ClaimsPrincipal? claims) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var data = context.Arguments.OfType<IEntityWithScopeId>().FirstOrDefault();
        if (data is null)
        {
            return Results.BadRequest("Entity with ScopeId required");
        }

        if (claims is null)
        {
            return Results.Unauthorized();
        }

        var role = claims.FindFirstValue(ClaimTypes.Role) ?? claims.FindFirstValue("role") ?? string.Empty;
        if (string.IsNullOrEmpty(role))
        {
            return Results.Unauthorized();
        }

        if (!await scopeAccessService.HasAccessAsync(data.ScopeId, role.Adapt<UserRole>()))
        {
            return Results.Json(new { error = "No access to scope" }, statusCode: StatusCodes.Status403Forbidden);
        }

        return await next.Invoke(context);
    }
}
