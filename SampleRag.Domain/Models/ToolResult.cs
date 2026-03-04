using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.Models;

public class ToolResult
{
    public AiTool Tool { get; set; }

    public object Value { get; set; }
}
