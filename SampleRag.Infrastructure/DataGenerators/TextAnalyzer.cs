using Mapster;
using Microsoft.SemanticKernel;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Enums;

namespace SampleRag.Infrastructure.DataGenerators;

public class TextAnalyzer(Kernel kernel) : ITextAnalyzer
{
    public async Task<string> DetectLanguageAsync(string text)
    {
        var args = new KernelArguments()
        {
            ["input"] = text,
        };

        var result = await kernel.InvokeAsync("TextAnalisys", "detect_language", args);
        return result.ToString() ?? string.Empty;
    }

    public async Task<UserQueryComplexity> DetermineQueryComplexity(string query)
    {
        var args = new KernelArguments(new PromptExecutionSettings()
        {
            ModelId = "qwen3.5:0.8b",
        })
        {
            ["input"] = query,
        };

        var result = await kernel.InvokeAsync("TextAnalisys", "detect_query_complexity", args);
        var value = result.ToString() ?? string.Empty;
        return value.Adapt<UserQueryComplexity>();
    }

    public async Task<string> TranslateText(string text, string targetLanguage)
    {
        var args = new KernelArguments(new PromptExecutionSettings()
        {
            ModelId = "qwen3.5:0.8b",
        })
        {
            ["input"] = text,
            ["language"] = targetLanguage,
        };

        var result = await kernel.InvokeAsync("TextAnalisys", "translate", args);
        return result.ToString() ?? string.Empty;
    }
}
