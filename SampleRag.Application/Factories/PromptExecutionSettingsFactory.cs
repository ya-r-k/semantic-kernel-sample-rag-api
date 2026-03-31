using System.Collections.Immutable;
using Microsoft.SemanticKernel;
using SampleRag.Domain.Interfaces.Factories;

namespace SampleRag.Application.Factories;

public class PromptExecutionSettingsFactory(
    ImmutableDictionary<string, KernelFunction[]> kernelFunctionsOptions) : ISettingsFactory<PromptExecutionSettings>
{
    public PromptExecutionSettings GetSettings(string settingsName, IDictionary<string, object>? outerArguments = default)
    {
        kernelFunctionsOptions.TryGetValue(settingsName, out var functions);

        var result = new PromptExecutionSettings();

        if (functions is not null && functions.Length > 0)
        {
            result.FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functions);
        }
        else
        {
            result.FunctionChoiceBehavior = FunctionChoiceBehavior.None();
        }

        return result;
    }

    /*private static KernelFunction[] CreateFunctionWithParameters(FunctionsOptionsPairs functionsOptionsPairs, IDictionary<string, object>? outerArgs = default)
    {
        var result = new List<KernelFunction>();

        foreach (var pair in functionsOptionsPairs)
        {
            var method = (Kernel kernel, KernelFunction currentFunction, KernelArguments currentArgs, CancellationToken cancellationToken) =>
            {
                if (outerArgs is not null)
                {
                    foreach (var pair in outerArgs)
                    {
                        currentArgs.Add(pair.Key, pair.Value);
                    }
                }

                return pair.Key.InvokeAsync(kernel, currentArgs, cancellationToken);
            };

            result.Add(KernelFunctionFactory.CreateFromMethod(method, pair.Value));
        }

        return [.. result];
    }*/
}
