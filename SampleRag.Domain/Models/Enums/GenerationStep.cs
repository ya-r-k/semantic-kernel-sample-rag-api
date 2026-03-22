namespace SampleRag.Domain.Models.Enums;

public enum GenerationStep
{
    Unknown = 0,
    AiThinking = 1,
    ToolUsing = 2,
    ToolResult = 3,
    ResponseMessage = 4,
    NewChatName = 5,
}
