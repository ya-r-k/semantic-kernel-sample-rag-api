using Mapster;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Enums;

namespace SampleRag.API.Filters;

public class RouteScopeAccessFilter(
    IKnowledgeScopeService scopeService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var scopeId = context.HttpContext.GetRouteValue("scopeId").Adapt<Guid>();
        if (scopeId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["scopeId"] = ["Scope ID is required"],
            });
        }

        var role = context.HttpContext.User.Adapt<UserRole>();
        if (role is UserRole.Admin or UserRole.SuperAdmin)
        {
            if (!await scopeService.HasScopeIdAsync(scopeId))
            {
                return Results.Json(new { error = "Scope not found!" }, statusCode: StatusCodes.Status404NotFound);
            }
        }
        else if (!await scopeService.HasAccessAsync(scopeId, role))
        {
            return Results.Json(new { error = "No access to scope!" }, statusCode: StatusCodes.Status403Forbidden);
        }

        return await next.Invoke(context);
    }
}
