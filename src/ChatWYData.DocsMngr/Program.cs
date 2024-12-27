using ChatWYData.ApiServices;
using ChatWYData.DocsMngr.Components;

var builder = WebApplication.CreateBuilder(args);

// .NET Aspire Add services defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<DocumentApiService>();
builder.Services.AddHttpClient<DocumentApiService>(
    static client => client.BaseAddress = new("https+http://documentapi"));

builder.Services.AddSingleton<DocProcessorWorkerApiService>();
builder.Services.AddHttpClient<DocProcessorWorkerApiService>(
    static client => client.BaseAddress = new("https+http://docworker"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
