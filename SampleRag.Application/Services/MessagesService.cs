using System.Linq.Expressions;
using Mapster;
using SampleRag.Domain.Entities.Db;
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
    public async IAsyncEnumerable<MessagePartResponse> GenerateAiResponce(SendMessageRequest request, string userId)
    {
        var userMessage = request.Adapt<Message>();

        if (userMessage.ChatId.Equals(Guid.Empty))
        {
            var chat = userMessage.Adapt<Chat>();
            await chatService.AddAsync(chat);

            yield return chat.Adapt<MessagePartResponse>();
        }

        await foreach (var part in GenerateAiMessage(userMessage))
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
        var messagesHistory = await messagesRepository.GetBatchByAsync(x => x.ChatId.Equals(userMessage.ChatId), 30);

        var aiMessage = new Message
        {
            AiGenerated = true,
            ChatId = userMessage.ChatId,
            Text = string.Empty,
        };

        var prevGenerationStep = GenerationStep.Unknown;

        await foreach (var part in dataGenerator.GenerateStreamingData(messagesHistory.Append(userMessage), "naive-rag"))
        {
            if (part.Step is GenerationStep.ResponseMessage)
            {
                aiMessage.Text += part.Text;
            }

            if (part.Step == prevGenerationStep)
            {
                part.Step = GenerationStep.Unknown;
            }
            else
            {
                prevGenerationStep = part.Step;
            }

            yield return part;
        }

        aiMessage.CreatedAt = DateTime.UtcNow;

        await messagesRepository.AddAsync([userMessage, aiMessage]);

        yield return new MessagePartResponse
        {
            CreatedAt = aiMessage.CreatedAt,
        };
    }
}
