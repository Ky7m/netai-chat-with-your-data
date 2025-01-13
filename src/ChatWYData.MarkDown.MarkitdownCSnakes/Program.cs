using ChatWYData.DocumentsApi.EndPoints;
using CSnakes.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// .NET Aspire Add services defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// add logging
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
builder.Logging.AddConsole();

// Add services to the container.

// add csnakes
var pythonBuilder = builder.Services.WithPython();
var home = Path.Join(Environment.CurrentDirectory, ".");
var venv = Path.Join(Environment.CurrentDirectory, $".venv-csnakes");
pythonBuilder
        .WithHome(home)
        .WithVirtualEnvironment(venv)
        .FromNuGet("3.12.4")
        .FromMacOSInstallerLocator("3.12")
        .FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")
        .WithPipInstaller();

builder.Services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironment>().Md());

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Run the installation of Python and dependencies
InstallPythonAndDependencies(app.Logger);

// .NET Aspire map default endpoints
app.MapDefaultEndpoints();

// map app endpoints
app.MapMarkdownCSnakesEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// log Environmental variables
app.Logger.LogInformation($"Environmental Variables\n >>\tPython3_ROOT_DIR: {Environment.GetEnvironmentVariable("Python3_ROOT_DIR")}");

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