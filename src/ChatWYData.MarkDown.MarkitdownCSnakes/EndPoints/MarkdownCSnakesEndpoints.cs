using ChatWYData.FileEntities;
using System.Collections;

namespace ChatWYData.DocumentsApi.EndPoints;

public static class MarkdownCSnakesEndpoints
{
    public static void MapMarkdownCSnakesEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/convert", async (HttpContext httpContext, FileUploadRequest fuRequest, ILogger<Program> logger, CSnakes.Runtime.IMd markdown) =>
        {
            logger.LogInformation($"Processing file: {fuRequest.FileName}");
            var markdownResult = string.Empty;

            var tmpDocsFolder = Path.Combine(Environment.CurrentDirectory, "tmpdocs");
            if (!Directory.Exists(tmpDocsFolder))
            {
                logger.LogInformation($"Creating folder: {tmpDocsFolder}");
                Directory.CreateDirectory(tmpDocsFolder);
            }

            var filePath = Path.Combine(Environment.CurrentDirectory, "tmpdocs", fuRequest.FileName);
            logger.LogInformation($"File Path: {filePath}");
            if (File.Exists(filePath))
            {
                logger.LogInformation($"Deleting existing file.");
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, fuRequest.FileBytes);

            logger.LogInformation($"Converting [{filePath}] to markdown");
            markdownResult = markdown.ConvertToMd(filePath);
            File.Delete(filePath);
            logger.LogInformation($"DONE converting [{filePath}] to markdown");

            return markdownResult;
        });

        // DEBUG ENDPOINTS
        routes.MapGet("/testmd", async (HttpContext httpContext, ILogger<Program> logger, CSnakes.Runtime.IMd markdown) =>
        {
            var message = $"markdown is not null: {markdown is not null}";
            return Results.Ok(message.ToString());
        });

        routes.MapGet("/", async (ILogger<Program> logger) =>
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            var currentDateTime = DateTime.Now;
            logger.LogInformation($"Hello from MarkdownEndpoints at {currentDateTime}");

            var envVars = new Dictionary<string, string>();
            foreach (DictionaryEntry envVar in environmentVariables)
            {
                var key = envVar.Key.ToString();
                var value = envVar.Value.ToString();
                envVars[key] = value;
            }

            var sortedEnvVars = envVars.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

            var response = new
            {
                Message = $"Hello from MarkdownEndpoints at {currentDateTime}",
                EnvironmentVariables = sortedEnvVars
            };

            return Results.Ok(response);
        });
    }
}

