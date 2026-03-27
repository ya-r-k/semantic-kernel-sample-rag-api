using Microsoft.SemanticKernel;
using SampleRag.Domain.Interfaces.Factories;
using FunctionsOptionsPairs = System.Collections.Generic.IDictionary<
    Microsoft.SemanticKernel.KernelFunction,
    Microsoft.SemanticKernel.KernelFunctionFromMethodOptions
>;
using FunctionsSettings = System.Collections.Generic.IDictionary<
    string,
    System.Collections.Generic.IDictionary<
        Microsoft.SemanticKernel.KernelFunction,
        Microsoft.SemanticKernel.KernelFunctionFromMethodOptions
    >
>;

namespace SampleRag.Application.Factories;

public class PromptExecutionSettingsFactory(
    FunctionsSettings kernelFunctionsOptions) : ISettingsFactory<PromptExecutionSettings>
{
    public PromptExecutionSettings GetSettings(string settingName, IDictionary<string, object>? outerArguments = default)
    {
        kernelFunctionsOptions.TryGetValue(settingName, out var currentFunctionsOptions);

        var result = new PromptExecutionSettings();

        if (currentFunctionsOptions is not null && currentFunctionsOptions.Keys.Count > 0)
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
