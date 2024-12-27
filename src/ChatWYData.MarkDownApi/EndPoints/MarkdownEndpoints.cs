using Azure.Core;
using ChatWYData.ApiServices;
using ChatWYData.FileEntities;
using OpenAI.Chat;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ChatWYData.DocumentsApi.EndPoints;

public static class MarkdownEndpoints
{
    public static void MapMarkdownEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", async (ILogger<Program> logger) =>
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            var currentDateTime = DateTime.Now;
            logger.LogInformation($"Hello from MarkdownEndpoints at {currentDateTime}");

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

        routes.MapPost("/generatemd", async (HttpContext httpContext,
            FileUploadRequest fuRequest,
            ILogger<Program> logger,
            ChatClient chatClient,
            MarkitdownBashApiService markitdownBashApiService,
            MarkidownCSnakesApiService markidownCSnakesApi) =>
        {
            logger.LogInformation($"Processing file: {fuRequest.FileName}");

            var markdownResult = string.Empty;
            if (fuRequest.FileName.EndsWith(".md"))
            {
                logger.LogInformation($"File is already in Markdown format. Skipping Markdown conversion.");
                markdownResult = Encoding.UTF8.GetString(fuRequest.FileBytes);
            }

            // validate if the file extension is an image
            else if (fuRequest.FileName.EndsWith(".png") || fuRequest.FileName.EndsWith(".jpg") || fuRequest.FileName.EndsWith(".jpeg"))
            {
                logger.LogInformation($"File is an image. Using LLM to generate the markdown.");

                var imageBinaryData = BinaryData.FromBytes(fuRequest.FileBytes);
                var imageContentPart = ChatMessageContentPart.CreateImagePart(imageBytes: imageBinaryData, imageBytesMediaType: fuRequest.FileContentType);

                var prompt = @$"Extract the text from the attached image. Generate a MarkDown that represents the text of the image. Only return the markdown. If the image has no text, return an empty markdown.";
                var messages = new List<ChatMessage>
                        {
                    new UserChatMessage(prompt),
                    new UserChatMessage(imageContentPart)
                        };

                var chatResponse = await chatClient.CompleteChatAsync(messages);
                markdownResult = chatResponse.Value.Content[0].Text;
            }
            else
            {
                // trying to convert the file using csnakes only in windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        logger.LogInformation("using csnakes markdown api service");
                        markdownResult = await markidownCSnakesApi.ConvertAsync(fuRequest);
                    }
                    catch (Exception exc)
                    {
                        logger.LogError(exc, "Error using python markdown api service");
                    }
                }
                else
                {
                    logger.LogInformation("Using bash markdown api service");
                    markdownResult = await markitdownBashApiService.ConvertAsync(fuRequest);
                }
            }
            var response = new FileUploadResponse
            {
                MarkDown = markdownResult
            };
            return response;
        });
    }


}

