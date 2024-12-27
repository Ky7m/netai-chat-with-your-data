using ChatWYData.ApiServices;
using ChatWYData.Workers.DocProcessor;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<DocumentApiService>();
builder.Services.AddHttpClient<DocumentApiService>(
    static client => client.BaseAddress = new("https+http://documentapi"));

builder.Services.AddSingleton<WorkerService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map the endpoint with logging
app.MapGet("/health", (WorkerService worker) =>
{
    return Results.Ok(worker.LastRun);
});

app.MapGet("/stop", (WorkerService worker) =>
{
    worker.Stop();
    return Results.Ok("Worker Service Stopped");
});

app.MapGet("/start", (WorkerService worker) =>
{
    worker.Start();
    return Results.Ok(worker.LastRun);
});

app.MapGet("/process", (WorkerService worker) =>
{
    worker.ProcessDocuments();
    return Results.Ok(worker.LastRun.ToString());
});

app.MapGet("/status", (WorkerService worker) =>
{
    return Results.Ok(worker.WorkerStatus().ToString());
});

using (var scope = app.Services.CreateScope())
{
    var worker = scope.ServiceProvider.GetRequiredService<WorkerService>();
    worker.Stop();  
}

app.Run();