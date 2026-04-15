using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.Interfaces;

public interface ITextAnalyzer
{
    Task<string> DetectLanguageAsync(string text);

    Task<UserQueryComplexity> DetermineQueryComplexity(string query);

    Task<string> TranslateText(string text, string targetLanguage);
}
