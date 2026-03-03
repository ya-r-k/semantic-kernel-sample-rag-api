namespace SampleRag.Domain.Models.Enums;

public enum GenerationStep
{
    Unknown = 0,
    AiThinking = 1,
    ToolUsing = 2,
    ResponseMessage = 3,
    NewChatName = 4,
}
