using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Repositories;
using SampleRag.Domain.Models.Configs;
using SampleRag.Domain.RequestModels;
using SampleRag.Infrastructure.Repositories.Mongo;

namespace SampleRag.Di.Registries;

public static class MongoPersistanceRegistry
{
    public static void ConfigureMongoDb(this IServiceCollection services, DbSettings dbSettings)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonClassMap.RegisterClassMap<Document>(cm =>
        {
            cm.AutoMap();
            cm.MapProperty(x => x.Vector).SetShouldSerializeMethod(_ => false);
        });
        BsonClassMap.RegisterClassMap<DocumentChunk>(cm =>
        {
            cm.AutoMap();
            cm.MapProperty(x => x.Vector).SetShouldSerializeMethod(_ => false);
            cm.MapProperty(x => x.ScopeIdValue).SetShouldSerializeMethod(_ => false);
        });
        services.AddSingleton(new MongoClient(dbSettings.ConnectionString).GetDatabase(dbSettings.DatabaseName));

        services.ConfigureMongoDbRepositories();
    }

    private static void ConfigureMongoDbRepositories(this IServiceCollection services)
    {
        services.AddTransient<IFilterRepository<Guid, DocumentChunk, GetDocumentChunksByModel>, DocumentChunkRepository>();
        services.AddTransient<IFilterRepository<Guid, Message, GetMessagesByModel>, MessageRepository>();
        services.AddTransient<IFilterRepository<Guid, Chat, GetChatsByModel>, ChatRepository>();
        services.AddTransient<IFilterRepository<Guid, Feedback, GetFeedbackByModel>, FeedbackRepository>();
        services.AddTransient<IDocumentRepository, DocumentRepository>();
        services.AddTransient<IKnowledgeScopeRepository, KnowledgeScopeRepository>();
        services.AddTransient<IChatRepository, ChatRepository>();
    }
}
