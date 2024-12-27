using Azure.Core;
using ChatWYData.FileEntities;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChatWYData.DocumentsApi.EndPoints;

public static class MarkitdownBashEndpoints
{
    public static void MapMarkitdownBashEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/convert", async (HttpContext httpContext, FileUploadRequest fuRequest, ILogger<Program> logger) =>
        {
            logger.LogInformation($"Processing file: {fuRequest.FileName}");

            var markdownResult = string.Empty;

            var sourceFilePath = Path.Combine(Environment.CurrentDirectory, "tmpdocs", fuRequest.FileName);
            var generatedMDFilePath = Path.Combine(Environment.CurrentDirectory, "tmpdocs", "output.md");
            logger.LogInformation($"Source File Path: {sourceFilePath}");
            logger.LogInformation($"GenMD File Path: {generatedMDFilePath}");
            File.Delete(sourceFilePath);
            File.Delete(generatedMDFilePath);

            logger.LogInformation($"Writing file to disk.");
            File.WriteAllBytes(sourceFilePath, fuRequest.FileBytes);
            logger.LogInformation($"File written to disk.");

            logger.LogInformation($"START converting [{sourceFilePath}] to markdown");

            var markitdownApp = "markitdown";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // read environment variable MARKITDOWN_DIR
                markitdownApp = Environment.GetEnvironmentVariable("MARKITDOWN_DIR");
            }

            List<string> arguments = [$"{markitdownApp} '{sourceFilePath}' > '{generatedMDFilePath}'"];
            var message = RunCommands(arguments, "run markitdown app", logger);

            // open the converted file a get the text
            if (File.Exists(generatedMDFilePath))
            {
                markdownResult = File.ReadAllText(generatedMDFilePath);
            }

            // delete both files
            File.Delete(sourceFilePath);
            File.Delete(generatedMDFilePath);
            logger.LogInformation($"DONE converting [{sourceFilePath}] to markdown");

            return markdownResult;
        });

        // DEBUG ENDPOINTS
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

        routes.MapGet("/cmd/{args}", async (string args, ILogger<Program> logger) =>
        {
            var message = RunCommand(args, "python", logger);
            return Results.Ok(message);
        });

        routes.MapGet("/pythonv", async (ILogger<Program> logger) =>
        {
            List<string> arguments = ["python3 --version", "python3 -m pip --version"];
            var message = RunCommands(arguments, "python", logger);
            return Results.Ok(message);
        });

        routes.MapGet("/install", async (ILogger<Program> logger) =>
        {
            List<string> arguments = [
                "apt update",
                "apt upgrade -y",
                "apt install -y python3-full",
                "apt install -y python3-pip",
                "apt install -y pipx",
                "pipx ensurepath",
                "pipx install markitdown",
                "python3 -m venv .venv",
                "pipx install markitdown",
                "chmod 777 tmpdocs"];

            var message = RunCommands(arguments, "python", logger);
            return Results.Ok(message);
        });
    }

    private static string RunCommands(List<string> arguments, string task, ILogger<Program> logger)
    {
        var message = "";
        foreach (var argument in arguments)
        {
            message += RunCommand(argument, task, logger);
        }
        return message.ToString();
    }

    private static string RunCommand(string arguments, string task, ILogger<Program> logger)
    {
        logger.LogInformation($"START RunCommand - {task}");
        logger.LogInformation($">> Command Args: {arguments}");

        arguments = $"-c \"{arguments}\"";

        var si = new ProcessStartInfo()
        {
            FileName = "/bin/bash",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        si.WorkingDirectory = Environment.CurrentDirectory;
        var process = new Process
        {
            StartInfo = si
        };
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        var message = "";
        if (process.ExitCode != 0)
        {
            message = $"Error {arguments}: {error}";
        }
        else
        {
            message = $"{arguments} successfully: {result}";
        }
        logger.LogInformation($">> Result: {message}");
        logger.LogInformation($"END RunCommand- {task}");
        return message + Environment.NewLine;
    }
}

