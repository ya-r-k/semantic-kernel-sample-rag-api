using Microsoft.SemanticKernel;

namespace SampleRag.Application.Filters.Invocation;

public class NonAiArgumentsApplyingFilter(IDictionary<string, object> nonAiArguments) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        foreach (var pair in nonAiArguments)
        {
            context.Arguments[pair.Key] = pair.Value;
        }

        await next.Invoke(context);
    }
}
