using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

namespace SampleRagAPI.RagAPI.Classes;

internal class LLMCommunicator
{
    private TextEmbedding myTextEmbedding;
    private Kernel myKernel;

    public LLMCommunicator()
    {
        myKernel = InitializeKernel();
        myTextEmbedding = new TextEmbedding();
    }

    public Kernel InitializeKernel()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services
            .AddOllamaChatCompletion("gemma3", new Uri("http://localhost:11434"))
            .AddOllamaTextEmbeddingGeneration("mxbai-embed-large", new Uri("http://localhost:11434"));

        return kernelBuilder.Build();
    }

    public async Task AddRecords()
    {
        var ollamaEmbedding = myKernel.Services.GetService<IEmbeddingGenerator>();
        Data data = new Data();

        var databaseInitializer = new DatabaseInitializer("Host=localhost;Port=5433;Username=postgres;Password=admin;Database=postgres;");
        await databaseInitializer.InitializeDatabaseAsync<Pokemon>();

        NpgsqlDataSourceBuilder dataSourceBuilder = new("Host=localhost;Port=5433;Username=postgres;Password=admin;Database=postgres;");
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();
        myVectorStoreRecordCollection = new PostgresVectorStoreRecordCollection<long, Pokemon>(dataSource, "pokemons");

        foreach (var record in data.RetrieveRecords())
        {
            await myTextEmbedding.GenerateEmbeddingAndUpsertAsync(ollamaEmbedding, myVectorStoreRecordCollection, record);
        }
    }

    public async Task Run(string userInput)
    {
        var ollamaEmbedding = myKernel.Services.GetService<ITextEmbeddingGenerationService>();
        string mostSimilarDocument = await myTextEmbedding.SearchAsync(ollamaEmbedding, myVectorStoreRecordCollection, userInput);

        var chatCompletionService = myKernel.Services.GetService<IChatCompletionService>();
        string prompt = $"You are a bot that makes Pokémon recommendations. Recommended Pokémon: {mostSimilarDocument}. User input: {userInput}.";

        var settings = new OllamaPromptExecutionSettings { MaxTokens = 4096 };
        var history = new ChatHistory(prompt);
        var responses = await chatCompletionService.GetChatMessageContentsAsync(history, settings, myKernel);

        foreach (var response in responses)
        {
            Console.WriteLine(response.Content);
        }
    }
}
