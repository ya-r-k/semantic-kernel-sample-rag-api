using System.Collections.Immutable;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
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
    public static void ConfigureKernel(this IServiceCollection services, GenAiProviderSettings lmConfig, AiPromptTemplatingSettings promptsConfig)
    {
        services.AddSingleton(lmConfig);

        var kernelBuilder = services.AddKernel()
            .AddOllamaChatCompletion(lmConfig.TextModel, new Uri(lmConfig.Url))
            .AddOllamaTextGeneration(lmConfig.TextModel, new Uri(lmConfig.Url))
            .AddOllamaEmbeddingGenerator(lmConfig.TextEmbeddingModel, new Uri(lmConfig.Url));

        kernelBuilder.Plugins
            .AddFromType<TimePlugin>()
            .AddFromType<RetrievalPlugin>();

        foreach (var pluginPathPair in promptsConfig.PluginPathes)
        {
            kernelBuilder.Plugins.AddFromPromptDirectoryYaml(
                Path.Combine(AppContext.BaseDirectory, pluginPathPair.Value),
                pluginPathPair.Key,
                new HandlebarsPromptTemplateFactory());
        }

        //.AddFromPromptDirectory("", "", new KernelPromptTemplateFactory());

        services.ConfigurePromptExecutionSettings();
        services.ConfigureGenerators();
    }

    private static void ConfigureGenerators(this IServiceCollection services)
    {
        services.AddTransient<IDataGenerator, SemanticKernelDataGenerator>();
        services.AddTransient<ITextAnalyzer, TextAnalyzer>();

        services.AddSingleton<IEmbeddingGenerator<DocumentChunk, Embedding<float>>, DocumentChunkEmbeddingGenerator>();
    }

    private static void ConfigurePromptExecutionSettings(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var transformedFunctions = kernel.Plugins
                .SelectMany(plugin => plugin.Select(f =>
                    KernelFunctionFactory.CreateFromMethod(
                        method: async (Kernel kernel, KernelFunction currentFunction, KernelArguments currentArgs, CancellationToken cancellationToken) =>
                        {
                            return await currentFunction.InvokeAsync(kernel, currentArgs, cancellationToken);
                        },
                        functionName: f.Name,
                        description: f.Description,
                        parameters: [.. f.Metadata.Parameters.Where(p => p.Name != "scopeId")],
                        returnParameter: f.Metadata.ReturnParameter))).ToArray();

            return new Dictionary<string, KernelFunction[]>
            {
                ["naive-rag"] = [.. transformedFunctions.Where(x => x.PluginName == nameof(TimePlugin) || x.PluginName == nameof(RetrievalPlugin))],
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
