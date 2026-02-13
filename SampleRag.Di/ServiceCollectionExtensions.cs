using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddTransient<IDataGenerator, DataGenerator>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IMessageService, MessageService>();

        // Configure IMongoDatabase
        var dbSettings = configuration.GetSection(nameof(DbSettings)).Get<DbSettings>();
        if (dbSettings is not null)
        {
            services.AddSingleton(new MongoClient(dbSettings.ConnectionString).GetDatabase(dbSettings.DatabaseName));
        }

        services.AddTransient<IRepository<int, DocumentData>, MongoBaseRepository<int, DocumentData>>();
        services.AddTransient<IRepository<int, MessageData>, MongoBaseRepository<int, MessageData>>();
        services.AddTransient<IRepository<int, ChatData>, MongoBaseRepository<int, ChatData>>();
        services.AddTransient<IRepository<int, KnowledgeGroupData>, MongoBaseRepository<int, KnowledgeGroupData>>();

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
