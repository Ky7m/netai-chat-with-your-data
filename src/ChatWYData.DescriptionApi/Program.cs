using OpenAI.Chat;
using OpenAI;
using ChatWYData.DocumentsApi.EndPoints;

var builder = WebApplication.CreateBuilder(args);

// .NET Aspire Add services defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();


// add logging
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
builder.Logging.AddConsole();

// Add services to the container.

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

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// .NET Aspire map default endpoints
app.MapDefaultEndpoints();

// map app endpoints
app.MapDescriptionEndpoints();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
