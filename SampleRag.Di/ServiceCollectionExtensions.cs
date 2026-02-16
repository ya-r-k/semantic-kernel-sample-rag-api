using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SampleRag.Application.Interfaces;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Application.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.Models.Configs;
using SampleRag.Infrastructure.Repositories.Files;
using SampleRag.Infrastructure.Repositories.Mongo;

namespace SampleRag.Di;

public static class ServiceCollectionExtensions
{
    public static void ConfigureDependencies(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddTransient<IDataGenerator, DataGenerator>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IMessageService<Guid>, MessageService>();
        services.AddTransient<IScopeAccessService, ScopeAccessService>();

        // Configure IMongoDatabase
        var dbSettings = configuration.GetSection(nameof(DbSettings)).Get<DbSettings>();
        if (dbSettings is not null)
        {
            // Program.cs — ДО создания MongoClient!
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            services.AddSingleton(new MongoClient(dbSettings.ConnectionString).GetDatabase(dbSettings.DatabaseName));
        }

        services.AddTransient<IRepository<Guid, DocumentData>, MongoBaseRepository<DocumentData>>();
        services.AddTransient<IRepository<Guid, MessageData>, MongoBaseRepository<MessageData>>();
        services.AddTransient<IRepository<Guid, ChatData>, MongoBaseRepository<ChatData>>();
        services.AddTransient<IRepository<Guid, KnowledgeGroupData>, MongoBaseRepository<KnowledgeGroupData>>();
        services.AddTransient<IScopeUserRepository, ScopeUserRepository>();

        services.ConfigureFileAccessLocalDependencies(environment);
    }

    public static void ConfigureFileAccessLocalDependencies(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSingleton(new FilesStorageSettings
        {
            BasePath = environment.WebRootPath,
        });

        services.AddTransient<IFileRepository, LocalFileRepository>();
    }
}
