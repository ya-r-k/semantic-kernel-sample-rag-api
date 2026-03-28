using System.Collections.Immutable;
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
        services.ConfigureGenerators();
    }

    private static void ConfigureGenerators(this IServiceCollection services)
    {
        services.AddTransient<IDataGenerator, SemanticKernelDataGenerator>();
        services.AddSingleton<IEmbeddingGenerator<DocumentChunk, Embedding<float>>, DocumentChunkEmbeddingGenerator>();
    }

    private static void ConfigurePromptExecutionSettings(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var plugins = kernel.Plugins.ToArray();

            var transformedFunctionsOptions = CreateKernelFunctionsOptions(
                plugins,
                parameter => parameter.Name != "scopeId");

            return new Dictionary<string, ImmutableDictionary<KernelFunction, KernelFunctionFromMethodOptions>>
            {
                ["naive-rag"] = transformedFunctionsOptions.ToImmutableDictionary(),
            }.ToImmutableDictionary();
        });

        services.AddTransient<ISettingsFactory<PromptExecutionSettings>, PromptExecutionSettingsFactory>();
    }

    private static IDictionary<KernelFunction, KernelFunctionFromMethodOptions> CreateKernelFunctionsOptions(KernelPlugin[] plugins, Predicate<KernelParameterMetadata> includeKernelParameter)
    {
        var functionsOptionsPairs = new Dictionary<KernelFunction, KernelFunctionFromMethodOptions>();

        foreach (var plugin in plugins)
        {
            foreach (var function in plugin)
            {
                functionsOptionsPairs.Add(function, new KernelFunctionFromMethodOptions()
                {
                    FunctionName = function.Name,
                    Description = function.Description,
                    Parameters = CreateParameterMetadataWithParameters(function.Metadata.Parameters, includeKernelParameter),
                    ReturnParameter = function.Metadata.ReturnParameter,
                });
            }
        }

        return functionsOptionsPairs;
    }

    /// <summary>
    /// Create a list of KernelParameterMetadata instances from the provided instances which only includes permitted parameters.
    /// </summary>
    private static List<KernelParameterMetadata> CreateParameterMetadataWithParameters(IReadOnlyList<KernelParameterMetadata> parameters, Predicate<KernelParameterMetadata> includeKernelParameter)
    {
        var parametersToInclude = new List<KernelParameterMetadata>();
        foreach (var parameter in parameters)
        {
            if (includeKernelParameter.Invoke(parameter))
            {
                parametersToInclude.Add(parameter);
            }
        }

        return parametersToInclude;
    }
}
