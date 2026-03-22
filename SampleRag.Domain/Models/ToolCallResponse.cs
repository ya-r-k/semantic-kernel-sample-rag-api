using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.Models;

public class ToolCallResponse
{
    public AiTool Tool { get; set; }

    public Dictionary<string, object>? Arguments { get; set; }
}
