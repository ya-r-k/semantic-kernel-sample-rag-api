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
        return result.ToString();
    }

    public async Task<UserQueryComplexity> DetermineQueryComplexity(string query)
    {
        var args = new KernelArguments()
        {
            ["input"] = query,
        };

        var result = await kernel.InvokeAsync("TextAnalisys", "detect_query_complexity", args);
        return result.ToString().Adapt<UserQueryComplexity>();
    }

    public async Task<string> TranslateText(string text, string targetLanguage)
    {
        var args = new KernelArguments()
        {
            ["input"] = text,
            ["language"] = targetLanguage,
        };

        var result = await kernel.InvokeAsync("TextAnalisys", "translate", args);
        return result.ToString() ?? string.Empty;
    }
}
