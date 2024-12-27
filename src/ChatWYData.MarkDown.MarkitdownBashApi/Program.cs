using ChatWYData.DocumentsApi.EndPoints;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Runtime.InteropServices;


var builder = WebApplication.CreateBuilder(args);

// .NET Aspire Add services defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// add logging
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
builder.Logging.AddConsole();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Run the installation of Python and dependencies
InstallPythonAndDependencies(app.Logger);

// .NET Aspire map default endpoints
app.MapDefaultEndpoints();

// map app endpoints
app.MapMarkitdownBashEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// enable the local folder named tmp to be part of the webapplication
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "tmpdocs")),
    RequestPath = "/tmpdocs"
});

app.Run();

static void InstallPythonAndDependencies(ILogger logger)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        logger.LogInformation("Non installing dependencies in Windows");
        return;
    }
    logger.LogInformation("Installing Python and dependencies...");

    var commands = new List<string>
        {
            "apt update",
            "apt upgrade -y",
            "apt install -y python3-full",
            "apt install -y python3-pip",
            "apt install -y pipx",
            "pipx ensurepath",
            "pipx install markitdown",
            "python3 -m venv .venv",
            "pipx install markitdown",
            "chmod 777 tmpdocs"
        };

    foreach (var command in commands)
    {
        RunCommand(command, logger);
    }

    logger.LogInformation("Python and dependencies installed successfully.");
}

static void RunCommand(string command, ILogger logger)
{
    var processStartInfo = new ProcessStartInfo
    {
        FileName = "/bin/bash",
        Arguments = $"-c \"{command}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = new Process { StartInfo = processStartInfo };
    process.Start();
    var result = process.StandardOutput.ReadToEnd();
    var error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
        logger.LogError($"Error executing command '{command}': {error}");
    }
    else
    {
        logger.LogInformation($"Command '{command}' executed successfully: {result}");
    }
}
