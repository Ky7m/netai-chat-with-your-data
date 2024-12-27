using ChatWYData.ApiServices;
using ChatWYData.DocumentsApi.EndPoints;
using ChatWYData.DocumentsApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// .NET Aspire Add services defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<MarkdownApiService>();
builder.Services.AddHttpClient<MarkdownApiService>(
    static client => client.BaseAddress = new("https+http://markdown"));

builder.Services.AddSingleton<VectorStoreApiService>();
builder.Services.AddHttpClient<VectorStoreApiService>(
    static client => client.BaseAddress = new("https+http://vectorstore"));

builder.Services.AddSingleton<DescriptionApiService>();
builder.Services.AddHttpClient<DescriptionApiService>(
    static client => client.BaseAddress = new("https+http://description"));

// Add DbContext service
builder.AddSqlServerDbContext<Context>("sqldb");

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// add logging
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
builder.Logging.AddConsole();

// add the reference for the docs blob
builder.AddAzureBlobClient("storagedocs");

// Add Azure OpenAI client
var azureOpenAiClientName = "openai";
builder.AddAzureOpenAIClient(azureOpenAiClientName);

var app = builder.Build();

// .NET Aspire map default endpoints
app.MapDefaultEndpoints();

// map app endpoints
app.MapDocumentEndpoints();
app.MapFileBatchEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// manage db
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    try
    {
        app.Logger.LogInformation("Ensure database created");
        context.Database.EnsureCreated();
    }
    catch (Exception exc)
    {
        app.Logger.LogError(exc, "Error creating database");
    }
    DbInitializer.Initialize(context);
}


app.Run();
