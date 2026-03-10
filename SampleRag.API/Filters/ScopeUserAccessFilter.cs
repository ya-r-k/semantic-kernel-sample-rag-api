using System.Security.Claims;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.API.Filters;

public class ScopeUserAccessFilter(
    IKnowledgeScopeService scopeAccessService,
    ClaimsPrincipal user) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var data = context.Arguments.OfType<IEntityWithScopeId>().FirstOrDefault();
        if (data is null)
        {
            return Results.BadRequest("Entity with ScopeId required");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }
        
        if (!await scopeAccessService.HasAccessAsync(data.ScopeId, userId))
        {
            return Results.Json(new { error = "No access to scope" }, statusCode: StatusCodes.Status403Forbidden);
        }
            
        return await next.Invoke(context);
    }
}
