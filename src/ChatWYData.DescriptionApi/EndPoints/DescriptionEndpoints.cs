using ChatWYData.DataEntities;
using OpenAI.Chat;
using System.Collections;

namespace ChatWYData.DocumentsApi.EndPoints;

public static class DescriptionEndpoints
{
    public static void MapDescriptionEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", async (ILogger<Program> logger) =>
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            var currentDateTime = DateTime.Now;
            logger.LogInformation($"Hello from Description Endpoints at {currentDateTime}");

            var envVars = new Dictionary<string, string>();
            foreach (DictionaryEntry envVar in environmentVariables)
            {
                envVars[envVar.Key.ToString()] = envVar.Value.ToString();
            }

            var sortedEnvVars = envVars.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

            var response = new
            {
                Message = $"Hello from MarkdownEndpoints at {currentDateTime}",
                EnvironmentVariables = sortedEnvVars
            };

            return Results.Ok(response);
        });

        routes.MapPost("/generate_desc", async (HttpContext httpContext,
            Document document,
            ILogger<Program> logger,
            ChatClient chatClient) =>
        {
            logger.LogInformation($"Generating desc for: {document.FileName}");
            
            var initialContent = document.FileMarkDown.Length > 2000 ? document.FileMarkDown.Substring(0, 2000) : document.FileMarkDown;

            // generate a description from the document content
            var prompt = @$"Generate a 2 line description from the content below. 
The content is in MarkDown format. 
The content represents the first 2000 characters of a document that was uploaded by a user. 
The document file name is: {document.FileName}.
The description should be generated using the following information:
[CONTENT START]
{initialContent}
[CONTENT END]";
            var chatResponse = await chatClient.CompleteChatAsync(prompt);
            var generatedDescription = chatResponse.Value.Content[0].Text;

            document.FileDescription = generatedDescription;

            logger.LogInformation($"DONE gen desc for: {document.FileName}");
            return document;
        });
    }
}

