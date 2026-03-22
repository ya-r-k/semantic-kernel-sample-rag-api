using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.Models;

public class ToolResultResponse
{
    public AiTool Tool { get; set; }

    public object? Value { get; set; }
}
