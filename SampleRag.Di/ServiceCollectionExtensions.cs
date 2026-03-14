using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Quartz;
using SampleRag.Application.Factories;
using SampleRag.Application.Jobs;
using SampleRag.Application.Plugins;
using SampleRag.Application.Services;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Factories;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models.Configs;
using SampleRag.Domain.RequestModels;
using SampleRag.Infrastructure.DataGenerators;
using SampleRag.Infrastructure.EmbeddingGenerators;
using SampleRag.Infrastructure.Repositories.Files;
using SampleRag.Infrastructure.Repositories.Mongo;
using SampleRag.Infrastructure.Repositories.Vector;
using ApiDocument = SampleRag.Domain.Entities.Document;

namespace SampleRag.Di;

public static class ServiceCollectionExtensions
{
    public static void ConfigureDependencies(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddTransient<IDataGenerator, SemanticKernelDataGenerator>();

        services.AddTransient<IDocumentChunkService, DocumentChunkService>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IMessagesService, MessagesService>();
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<IKnowledgeScopeService, KnowledgeScopeService>();
        services.AddTransient<IFeedbackService, FeedbackService>();

        // Configure IMongoDatabase
        var dbSettings = configuration.GetSection(nameof(DbSettings)).Get<DbSettings>();
        if (dbSettings is not null)
        {
            services.ConfigureMongoDb(dbSettings);
            services.ConfigureQuartzJobs(dbSettings);
        }

        var jobsSettings = configuration.GetSection(nameof(DocumentsJobsSettings)).Get<DocumentsJobsSettings>();
        if (jobsSettings is not null)
        {
            services.AddSingleton(jobsSettings);
        }

        services.AddTransient<IFilterRepository<Guid, DocumentChunk, GetDocumentChunksByModel>, DocumentChunkRepository>();
        services.AddTransient<IFilterRepository<Guid, ApiDocument, GetDocumentsByModel>, DocumentRepository>();
        services.AddTransient<IFilterRepository<Guid, Message, GetMessagesByModel>, MessageRepository>();
        services.AddTransient<IFilterRepository<Guid, Chat, GetChatsByModel>, ChatRepository>();
        services.AddTransient<IFilterRepository<Guid, KnowledgeScope, GetBatchByModel>, KnowledgeScopeRepository>();
        services.AddTransient<IFilterRepository<Guid, Feedback, GetFeedbackByModel>, FeedbackRepository>();
        services.AddTransient<IKnowledgeScopeUserRepository, KnowledgeScopeUserRepository>();

        services.AddTransient<IVectorRepository<DocumentChunk>, QdrantDocumentChunkRepository>();

        services.ConfigureFileAccessLocalDependencies(environment);
    }

    public static void ConfigureMongoDb(this IServiceCollection services, DbSettings dbSettings)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonClassMap.RegisterClassMap<ApiDocument>(cm =>
        {
            cm.AutoMap();
            cm.MapProperty(x => x.Vector).SetShouldSerializeMethod(_ => false);
        });
        BsonClassMap.RegisterClassMap<DocumentChunk>(cm =>
        {
            cm.AutoMap();
            cm.MapProperty(x => x.Vector).SetShouldSerializeMethod(_ => false);
        });
        services.AddSingleton(new MongoClient(dbSettings.ConnectionString).GetDatabase(dbSettings.DatabaseName));
    }

    public static void ConfigureQuartzJobs(this IServiceCollection services, DbSettings dbSettings)
    {
        services.AddQuartz(q =>
        {
            q.AddJob<DocumentChunkingJob>(options => options.WithIdentity("chunk-documents"));
            q.AddTrigger(options => options
                .ForJob("chunk-documents")
                .WithIdentity("chunk-documents-trigger")
                .WithCronSchedule("0 0/5 11-23,0-7 * * ?")
                .WithDescription("Чанкинг документов каждые 30 сек с 21:00 до 08:00"));

            q.AddJob<ChunkVectorizationJob>(options => options.WithIdentity("vectorize-chunks"));
            q.AddTrigger(options => options
                .ForJob("vectorize-chunks")
                .WithIdentity("vectorize-chunks-trigger")
                .WithCronSchedule("0 0/2 11-23,0-7 * * ?")
                .WithDescription("Ночное задание каждые 2 минуты"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    public static void ConfigureFileAccessLocalDependencies(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSingleton(new FilesStorageSettings
        {
            BasePath = environment.WebRootPath,
        });

        services.AddTransient<IFileRepository, LocalFileRepository>();
    }

    public static void ConfigureAiDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        var vectorDbSettings = configuration.GetSection(nameof(VectorDbSettings)).Get<VectorDbSettings>();
        var lmConfig = configuration.GetSection(nameof(GenAiProviderSettings)).Get<GenAiProviderSettings>();

        services.ConfigureKernel(lmConfig ?? new ());
        services.ConfigureQdrant(vectorDbSettings ?? new ());
    }

    public static void ConfigureKernel(this IServiceCollection services, GenAiProviderSettings lmConfig)
    {
        services.AddSingleton(lmConfig);

        var kernelBuilder = services.AddKernel()
            .AddOllamaChatCompletion(lmConfig!.TextModel, new Uri(lmConfig.Url))
            .AddOllamaTextGeneration(lmConfig!.TextModel, new Uri(lmConfig.Url))
            .AddOllamaEmbeddingGenerator(lmConfig.TextEmbeddingModel, new Uri(lmConfig.Url));

        kernelBuilder.Plugins
            .AddFromType<TimePlugin>()
            .AddFromType<RetrievalPlugin>();

        services.ConfigurePromptExecutionSettings();

        // Configures Semantic Memory
        // services.AddKernelMemory<MemoryServerless>();
        services.AddSingleton<IEmbeddingGenerator<DocumentChunk, Embedding<float>>, DocumentChunkEmbeddingGenerator>();
    }

    public static void ConfigureQdrant(this IServiceCollection services, VectorDbSettings vectorDbSettings)
    {
        services.AddSingleton(vectorDbSettings);

        var qdrantClient = new QdrantClient(new Uri(vectorDbSettings!.Url));

        services.AddQdrantVectorStore(
            _ => qdrantClient,
            sp => new QdrantVectorStoreOptions
            {
                EmbeddingGenerator = sp.GetService<IEmbeddingGenerator>(),
                HasNamedVectors = false,
            })
            .AddQdrantCollection<Guid, DocumentChunk>("document-chunks")
            .AddQdrantCollection<Guid, ApiDocument>("documents")
            .AddQdrantCollection<Guid, KnowledgeScope>("knowledge-groups");

        EnsureCollectionsExistsAsync(qdrantClient, vectorDbSettings).Wait();
    }

    public static async Task EnsureCollectionsExistsAsync(this QdrantClient qdrantClient, VectorDbSettings vectorDbSettings)
    {
        var collections = await qdrantClient.ListCollectionsAsync();

        foreach (var collectionConfig in vectorDbSettings.Collections)
        {
            if (collections.Any(x => string.Equals(x, collectionConfig.CollectionName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var quantizationConfig = GetQuantizationConfig(collectionConfig.Quantization);

            await qdrantClient.RecreateCollectionAsync(collectionConfig.CollectionName, new VectorParams
            {
                Size = collectionConfig.VectorSize,
                Distance = collectionConfig.Distance.Adapt<Distance>(),
                QuantizationConfig = quantizationConfig,
            });
        }
    }

    private static void ConfigurePromptExecutionSettings(this IServiceCollection services)
    {
        var executionSettingsBase = new Dictionary<string, PromptExecutionSettings>()
        {
            ["with-auto-choosing-functions"] = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            },
        };

        services.AddSingleton<ISettingsFactory<PromptExecutionSettings>>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var retrievalPlugin = kernel.Plugins.GetFunction(nameof(RetrievalPlugin), "RetrieveRelevantChunks");

            var executionSettings = new Dictionary<string, PromptExecutionSettings>(executionSettingsBase)
            {
                ["naive-rag"] = new PromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Required([retrievalPlugin]),
                },
            };

            return new PromptExecutionSettingsFactory(executionSettings);
        });
    }

    private static QuantizationConfig GetQuantizationConfig(string quantization)
    {
        return quantization switch
        {
            "Binary" => new QuantizationConfig()
            {
                Binary = new BinaryQuantization()
                {
                    QueryEncoding = new BinaryQuantizationQueryEncoding()
                    {
                        Setting = BinaryQuantizationQueryEncoding.Types.Setting.Scalar8Bits,
                    },
                    Encoding = BinaryQuantizationEncoding.TwoBits,
                },
            },
            "Scalar" => new QuantizationConfig()
            {
                Scalar = new ScalarQuantization()
                {
                    Quantile = 0.98f,
                    Type = QuantizationType.Int8,
                },
            },
            "Product" => new QuantizationConfig()
            {
                Product = new ProductQuantization()
                {
                    Compression = CompressionRatio.X4,
                },
            },
            _ => throw new NotImplementedException(),
        };
    }
}
