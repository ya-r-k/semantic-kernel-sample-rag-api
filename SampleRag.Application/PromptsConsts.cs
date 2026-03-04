namespace SampleRag.Application;

public static class PromptsConsts
{
    public const string GenerateChatNamePromptYaml = @"
name: GenerateChatName
template: |
  {{$chat_history}}
  
  Respond ONLY with JSON:
  {""title"": ""3-8 words title here""}
template_format: semantic-kernel
input_variables:
  - name: chat_history
    description: Chat history
    is_required: true
execution_settings:
  default:
    temperature: 0.0
    max_tokens: 40

";
}
