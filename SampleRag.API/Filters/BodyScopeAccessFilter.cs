using Mapster;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Abstractions;
using SampleRag.Domain.Models.Enums;

namespace SampleRag.API.Filters;

public class BodyScopeAccessFilter(
    IKnowledgeScopeService scopeService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var data = context.Arguments.OfType<IEntityWithScopeId>().FirstOrDefault();
        if (data is null || data.ScopeId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["scopeId"] = ["Scope ID is required"],
            });
        }

        var role = context.HttpContext.User.Adapt<UserRole>();
        if (role is UserRole.Admin or UserRole.SuperAdmin)
        {
            if (!await scopeService.HasScopeIdAsync(data.ScopeId))
            {
                return Results.Json(new { error = "Scope not found!" }, statusCode: StatusCodes.Status404NotFound);
            }
        }
        else if (!await scopeService.HasAccessAsync(data.ScopeId, role))
        {
            return Results.Json(new { error = "No access to scope!" }, statusCode: StatusCodes.Status403Forbidden);
        }

        return await next.Invoke(context);
    }
}
