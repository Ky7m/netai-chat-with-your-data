using Azure.Search.Documents.Indexes;
using ChatWYData.VectorEntities;
using ChatWYData.VectorStoreAzureAISearch;
using ChatWYData.VectorStoreAzureAISearch.EndPoints;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add DbContext service
builder.AddSqlServerDbContext<Context>("sqldb");

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// To reuse existing Azure AI Search resources, this to "azureaisearchdev", and check the documentation on how to reuse the resources
var azureAiSearchName = "azureaisearch";
builder.AddAzureSearchClient(azureAiSearchName);

// in dev scenarios rename this to "openaidev", and check the documentation to reuse existing AOAI resources
var azureOpenAiClientName = "openai";
builder.AddAzureOpenAIClient(azureOpenAiClientName);

// get azure openai client and create Chat client from aspire hosting configuration
builder.Services.AddSingleton(serviceProvider =>
{
    var chatDeploymentName = "gpt-4o";
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Chat client configuration, modelId: {chatDeploymentName}");
    ChatClient chatClient = null;
    try
    {
        OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
        chatClient = client.GetChatClient(chatDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating embeddings client");
    }
    return chatClient;
});

// get azure openai client and create embedding client from aspire hosting configuration
builder.Services.AddSingleton(serviceProvider =>
{
    var embeddingsDeploymentName = "text-embedding-ada-002";
    var logger = serviceProvider.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Embeddings client configuration, modelId: {embeddingsDeploymentName}");
    EmbeddingClient embeddingsClient = null;
    try
    {
        OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
        embeddingsClient = client.GetEmbeddingClient(embeddingsDeploymentName);
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating embeddings client");
    }
    return embeddingsClient;
});

builder.Services.AddSingleton<IConfiguration>(sp =>
{
    return builder.Configuration;
});

// add memory context
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context");

    var azureSearchIndexClient = sp.GetRequiredService<SearchIndexClient>();
    logger.LogInformation($"Azure Search Index Client ServiceName: {azureSearchIndexClient.ServiceName}");

    var vectorProductStore = new AzureAISearchVectorStore(azureSearchIndexClient);
    var documentsCollection = vectorProductStore.GetCollection<string, DocumentVector>("documents");    
    documentsCollection.CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();

    logger.LogInformation("DONE memory context created");
    return documentsCollection;
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// map vector interactions
app.MapVectorStoreEndpoints();

app.UseAuthorization();

app.MapControllers();

app.Run();
