using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SampleRag.Application.Plugins;
using SampleRag.Domain.Entities.Db;
using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Enums;
using SampleRag.Domain.RequestModels;
using System.Security.Claims;
using OllamaChatResponseStream = OllamaSharp.Models.Chat.ChatResponseStream;
using OllamaFunction = OllamaSharp.Models.Chat.Message.Function;
using OllamaToolCall = OllamaSharp.Models.Chat.Message.ToolCall;

namespace SampleRag.Di.Mapping;

public static class MappingServiceCollectionExtensions
{
    public static IServiceCollection ConfigureMapster(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<Message, ChatMessageContent>()
            .Map(dest => dest.Content, src => src.Text)
            .Map(dest => dest.Role, src => GetAuthorRole(src))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<StreamingChatMessageContent, MessagePartResponse>()
            .Map(dest => dest.Text, src => GetMessagePartText(src))
            .Map(dest => dest.Step, src => GetMessagePartGenerationStep(src))
            .Map(dest => dest.ToolsCalls, src => GetTools(src))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<OllamaToolCall, OllamaFunction>()
            .MapWith(src => src.Function ?? new ())
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<OllamaToolCall, ToolCallResponse>()
            .MapWith(src => src.Function.Adapt<ToolCallResponse>())
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<OllamaFunction, ToolCallResponse>()
            .Map(dest => dest.Tool, src => ParseAiTool(src.Name))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<ChatHistory, MessagePartResponse>()
            .Map(dest => dest.ToolsResults, src => GetToolsResults(src))
            .Map(dest => dest.Step, src => GenerationStep.ToolResult)
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<ToolResultResponse[], SourceReference[]>()
            .MapWith(src => GetSourceReferences(src))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<string?, AiTool>()
            .MapWith(src => ParseAiTool(src))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<Message, Chat>()
            .Map(dest => dest.Name, src => string.Concat(src.Text.Take(80)))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<Chat, MessagePartResponse>()
            .Map(dest => dest.Text, src => src.Name)
            .Map(dest => dest.Step, src => GenerationStep.NewChatName)
            .Map(dest => dest.NewChatId, src => src.Id)
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<(CreateChatRequest request, ClaimsPrincipal claims), Chat>()
            .MapWith(src => src.request.Adapt<Chat>())
            .Map(dest => dest.OwnerIds, src => GetOwnerIds(src.request, src.claims))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<ClaimsPrincipal, string>()
            .MapWith(src => src.FindFirstValue(ClaimTypes.NameIdentifier) ?? src.FindFirstValue("sub"))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<(CreateScopeRequest scope, ClaimsPrincipal claims), CreateScopeRequest>()
            .MapWith(src => src.scope)
            .Map(dest => dest.UsersIds, src => src.scope.UsersIds.Append(src.claims.Adapt<string>()))
            .Compile();

        TypeAdapterConfig.GlobalSettings.NewConfig<(CreateScopeRequest[] scopes, ClaimsPrincipal claims), CreateScopeRequest[]>()
            .MapWith(src => src.scopes.Select(x => new CreateScopeRequest
            {
                Name = x.Name,
                UsersIds = x.UsersIds.Append(src.claims.Adapt<string>()).ToArray(),
            }).ToArray())
            .Compile();

        return services;
    }

    private static SourceReference[] GetSourceReferences(ToolResultResponse[] src)
    {
        return src.FirstOrDefault(x => x.Tool == AiTool.InternalDocumentData)?.Value.Adapt<SourceReference[]>() ?? [];
    }

    private static string[] GetOwnerIds(CreateChatRequest request, ClaimsPrincipal claims)
    {
        var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? claims.FindFirstValue("sub") ?? "unknown";

        return request.OwnerIds?.Length > 0 ? [.. request.OwnerIds, userId] : [userId];
    }

    private static AuthorRole GetAuthorRole(Message src)
    {
        return src.AiGenerated switch
        {
            true => AuthorRole.Assistant,
            false => AuthorRole.User,
        };
    }

    private static string GetMessagePartText(StreamingChatMessageContent src)
    {
        return src switch
        {
            var y when y.InnerContent is OllamaChatResponseStream innerContent &&
                innerContent?.Message?.Thinking is not null => innerContent.Message.Thinking,
            var y when y?.Content is not null => y.Content,
            _ => string.Empty,
        };
    }

    private static GenerationStep GetMessagePartGenerationStep(StreamingChatMessageContent src)
    {
        return src switch
        {
            var y when y.InnerContent is OllamaChatResponseStream innerContent &&
                innerContent?.Message?.Thinking is not null => GenerationStep.AiThinking,
            var y when y.InnerContent is OllamaChatResponseStream innerContent &&
                innerContent?.Message?.ToolCalls is not null => GenerationStep.ToolUsing,
            var y when y?.Content is not null => GenerationStep.ResponseMessage,
            _ => GenerationStep.Unknown,
        };
    }

    private static IEnumerable<OllamaToolCall>? GetTools(StreamingChatMessageContent src)
    {
        if (src.InnerContent is not OllamaChatResponseStream innerContent ||
            innerContent?.Message?.ToolCalls is null ||
            !innerContent.Message.ToolCalls.Any())
        {
            return default;
        }

        return innerContent.Message.ToolCalls;
    }

    private static IEnumerable<ToolResultResponse>? GetToolsResults(ChatHistory src)
    {
        return src.Where(x => x.Role == AuthorRole.Tool)
            .SelectMany(x => x.Items)
            .Select(x => (x as FunctionResultContent)?.Result)
            .Where(x => x != null)
            .GroupBy(GetToolKey)
            .Select(x => new ToolResultResponse
            {
                Tool = x.Key,
                Value = x.Key switch
                {
                    AiTool.InternalDocumentData => x.SelectMany(x => x as IEnumerable<DocumentChunk> ?? [])
                        .DistinctBy(x => new { x.DocumentId, x.PageNumber })
                        .OrderBy(x => x.DocumentId)
                        .ThenBy(x => x.PageNumber)
                        .ToArray()
                        .Adapt<SourceReference[]>(),
                    _ => x.Cast<object>().ToArray(),
                },
            });
    }

    private static AiTool GetToolKey(object? src)
    {
        return src switch
        {
            IEnumerable<DocumentChunk> => AiTool.InternalDocumentData,
            string => AiTool.CurrentTime,
            _ => AiTool.Unknown
        };
    }

    private static AiTool ParseAiTool(string? src)
    {
        return src switch
        {
            $"{nameof(RetrievalPlugin)}_RetrieveRelevantChunks" => AiTool.InternalDocumentData,
            $"{nameof(TimePlugin)}_GetCurrentTime" => AiTool.CurrentTime,
            _ => AiTool.Unknown,
        };
    }
}
