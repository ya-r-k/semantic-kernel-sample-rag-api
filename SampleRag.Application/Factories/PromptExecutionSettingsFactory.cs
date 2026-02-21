using Microsoft.SemanticKernel;
using SampleRag.Domain.Interfaces.Factories;

namespace SampleRag.Application.Factories;

public class PromptExecutionSettingsFactory(
    IDictionary<string, PromptExecutionSettings> settings) : ISettingsFactory<PromptExecutionSettings>
{
    public PromptExecutionSettings GetSettings(string settingsName)
    {
        if (!settings.TryGetValue(settingsName, out var result))
        {
            result = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };
        }

        return result;
    }
}
