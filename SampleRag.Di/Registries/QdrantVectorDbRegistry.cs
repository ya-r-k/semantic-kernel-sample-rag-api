using Mapster;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SampleRag.Domain.Entities;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Configs;
using SampleRag.Infrastructure.Repositories.Vector;
using ApiDocument = SampleRag.Domain.Entities.Document;

namespace SampleRag.Di.Registries;

public static class QdrantVectorDbRegistry
{
    public static void ConfigureQdrant(this IServiceCollection services, VectorDbSettings vectorDbSettings)
    {
        services.ConfigureVectorRepositories();

        services.AddSingleton(vectorDbSettings);
        var qdrantClient = new QdrantClient(new Uri(vectorDbSettings.Url));

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

    private static void ConfigureVectorRepositories(this IServiceCollection services)
    {
        services.AddTransient<IVectorRepository<DocumentChunk>, QdrantDocumentChunkRepository>();
    }

    private static async Task EnsureCollectionsExistsAsync(this QdrantClient qdrantClient, VectorDbSettings vectorDbSettings)
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
