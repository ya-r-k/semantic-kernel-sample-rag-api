using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SampleRag.Application.Factories;
using SampleRag.Application.Plugins;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Factories;
using SampleRag.Domain.Models.Configs;
using SampleRag.Infrastructure.DataGenerators;
using SampleRag.Infrastructure.EmbeddingGenerators;

namespace SampleRag.Di.Registries;

public static class SemanticKernelRegistry
{
    public static void ConfigureKernel(this IServiceCollection services, GenAiProviderSettings lmConfig)
    {
        services.AddSingleton(lmConfig);

        var kernelBuilder = services.AddKernel()
            .AddOllamaChatCompletion(lmConfig.TextModel, new Uri(lmConfig.Url))
            .AddOllamaTextGeneration(lmConfig.TextModel, new Uri(lmConfig.Url))
            .AddOllamaEmbeddingGenerator(lmConfig.TextEmbeddingModel, new Uri(lmConfig.Url));

        kernelBuilder.Plugins
            .AddFromType<TimePlugin>()
            .AddFromType<RetrievalPlugin>();

        services.ConfigurePromptExecutionSettings();

        // Configures Semantic Memory
        // services.AddKernelMemory<MemoryServerless>();
        services.ConfigureGenerators();
    }

    private static void ConfigureGenerators(this IServiceCollection services)
    {
        services.AddTransient<IDataGenerator, SemanticKernelDataGenerator>();
        services.AddTransient<IEmbeddingGenerator<DocumentChunk, Embedding<float>>, DocumentChunkEmbeddingGenerator>();
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
}
