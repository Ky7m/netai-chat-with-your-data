using ChatWYData.ApiServices;
using ChatWYData.ChatApp.Components;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

builder.Services.AddSingleton<VectorStoreApiService>();
builder.Services.AddHttpClient<VectorStoreApiService>(
    static client => client.BaseAddress = new("https+http://vectorstore"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Saas compiler for css styles and files
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSassCompiler();
}

builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();