using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleRag.Application.Services;
using SampleRag.Domain.Interfaces.Services;

namespace SampleRag.Di.Registries;

public static class BusinessLogicRegistry
{
    public static void ConfigureBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDocumentChunkService, DocumentChunkService>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IMessagesService, MessagesService>();
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<IKnowledgeScopeService, KnowledgeScopeService>();
        services.AddTransient<IFeedbackService, FeedbackService>();
    }
}
