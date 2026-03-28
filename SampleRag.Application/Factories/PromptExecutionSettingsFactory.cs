using Microsoft.SemanticKernel;
using SampleRag.Domain.Interfaces.Factories;
using FunctionsOptionsPairs = System.Collections.Immutable.ImmutableDictionary<
    Microsoft.SemanticKernel.KernelFunction,
    Microsoft.SemanticKernel.KernelFunctionFromMethodOptions
>;
using FunctionsSettings = System.Collections.Immutable.ImmutableDictionary<
    string,
    System.Collections.Immutable.ImmutableDictionary<
        Microsoft.SemanticKernel.KernelFunction,
        Microsoft.SemanticKernel.KernelFunctionFromMethodOptions
    >
>;

namespace SampleRag.Application.Factories;

public class PromptExecutionSettingsFactory(
    FunctionsSettings kernelFunctionsOptions) : ISettingsFactory<PromptExecutionSettings>
{
    public PromptExecutionSettings GetSettings(string settingsName, IDictionary<string, object>? outerArguments = default)
    {
        kernelFunctionsOptions.TryGetValue(settingsName, out var currentFunctionsOptions);

        var result = new PromptExecutionSettings();

        if (currentFunctionsOptions is not null && !currentFunctionsOptions.IsEmpty)
        {
            var transformedFunctions = CreateFunctionWithParameters(currentFunctionsOptions, outerArguments);
            result.FunctionChoiceBehavior = FunctionChoiceBehavior.Required(transformedFunctions);
        }
        else
        {
            result.FunctionChoiceBehavior = FunctionChoiceBehavior.None();
        }

        return result;
    }

    private static KernelFunction[] CreateFunctionWithParameters(FunctionsOptionsPairs functionsOptionsPairs, IDictionary<string, object>? outerArgs = default)
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
    }
}
