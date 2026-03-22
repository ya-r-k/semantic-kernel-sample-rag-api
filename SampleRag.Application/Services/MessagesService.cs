using System.Text;
using Mapster;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Enums;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class MessagesService(
    IDataGenerator dataGenerator,
    IChatService chatService,
    IFilterRepository<Guid, Message, GetMessagesByModel> messagesRepository) : IMessagesService
{
    public async IAsyncEnumerable<MessagePartResponse> GenerateAiResponce(SendMessageRequest message, string userId)
    {
        var userMessage = message.Adapt<Message>();
        if (userMessage.ChatId == Guid.Empty)
        {
            var newChat = new Chat
            {
                Name = string.Concat(message.Text.Take(80)),
                OwnerIds = [userId],
            };

            var createdChats = await chatService.AddAsync(newChat);
            var createdChat = createdChats.FirstOrDefault();
            if (createdChat is null)
            {
                yield break;
            }

            userMessage.ChatId = createdChat.Id;
            yield return createdChat.Adapt<MessagePartResponse>();
        }

        await foreach (var part in this.GenerateAiMessage(userMessage))
        {
            yield return part;
        }
    }

    public async Task<IEnumerable<Message>> GetBatchByAsync(GetMessagesByModel model)
    {
        return await messagesRepository.GetBatchByAsync(model);
    }

    private async IAsyncEnumerable<MessagePartResponse> GenerateAiMessage(Message userMessage)
    {
        userMessage.CreatedAt = DateTime.UtcNow;
        var messagesHistory = await messagesRepository.GetBatchByAsync(new GetMessagesByModel
        {
            ChatId = userMessage.ChatId,
            BatchSize = 30,
        });

        var aiMessage = new Message
        {
            AiGenerated = true,
            ChatId = userMessage.ChatId,
        };

        var aiTextBuilder = new StringBuilder();
        await foreach (var part in dataGenerator.GenerateStreamingData(messagesHistory.Append(userMessage), "naive-rag"))
        {
            if (part.Step is GenerationStep.ResponseMessage)
            {
                aiTextBuilder.Append(part.Text);
            }
            else if (part.Step is GenerationStep.ToolResult)
            {
                aiMessage.SourceReferences = part.ToolsResults.Adapt<SourceReference[]>();
            }

            /*if (part.Step == prevGenerationStep)
            {
                part.Step = GenerationStep.Unknown;
            }
            else
            {
                prevGenerationStep = part.Step;
            }*/

            yield return part;
        }

        aiMessage.CreatedAt = DateTime.UtcNow;
        aiMessage.Text = aiTextBuilder.ToString();

        await messagesRepository.AddAsync([userMessage, aiMessage]);

        yield return new MessagePartResponse
        {
            CreatedAt = aiMessage.CreatedAt,
        };
    }
}
