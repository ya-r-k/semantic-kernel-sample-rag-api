using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using OllamaSharp;
using Qdrant.Client;
using SampleRag.API.Endpoints;
using SampleRag.Di;
using SampleRag.Domain.Models.Configs;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateSlimBuilder(args);

//builder.AddServiceDefaults();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 4;  // Максимум 4 запроса
        opt.Window = TimeSpan.FromSeconds(12);  // За 12 секунд
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;  // Очередь на 2 запроса
    });

    // Обработка превышения лимита
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests", token);
    };
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

/*builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});*/

var lmConfig = builder.Configuration.GetSection(nameof(GenAiProviderSettings)).Get<GenAiProviderSettings>();
var vectorConfig = builder.Configuration.GetSection(nameof(VectorDbSettings)).Get<VectorDbSettings>();

builder.Services.AddQdrantVectorStore(_ => new QdrantClient(new Uri(vectorConfig!.Url)),
    _ => new QdrantVectorStoreOptions
    {
        EmbeddingGenerator = new OllamaApiClient(lmConfig!.TextModel, lmConfig.TextEmbeddingModel),
        HasNamedVectors = false,
    });

builder.Services.AddKernel()
    .AddOllamaChatCompletion(lmConfig!.TextModel, new Uri(lmConfig.Url))
    .AddOllamaTextGeneration(lmConfig!.TextModel, new Uri(lmConfig.Url))
    .AddOllamaEmbeddingGenerator(lmConfig.TextEmbeddingModel, new Uri(lmConfig.Url));

builder.Services.ConfigureDependencies(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "no-sniff";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapChatsEndpoints();
app.MapMessagesEndpoints();
app.MapDocumentsEndpoints();
app.MapFilesEndpoints();
app.MapKnowledgeGroupsEndpoints();

app.Run();
