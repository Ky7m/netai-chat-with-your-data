using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var appInsights = builder.AddAzureApplicationInsights("appInsights");

var chatDeploymentName = "gpt-4o";
var embeddingsDeploymentName = "text-embedding-ada-002";
var aoai = builder.AddAzureOpenAI("openai")
    .AddDeployment(new AzureOpenAIDeployment(chatDeploymentName,
    "gpt-4o", //"gpt-4o-mini",
    "2024-05-13", //"2024-07-18", 
    "GlobalStandard",
    10))
    .AddDeployment(new AzureOpenAIDeployment(embeddingsDeploymentName,
    "text-embedding-ada-002",
    "2"));

var azureaisearch = builder.AddAzureSearch("azureaisearch");

var sqldb = builder.AddSqlServer("sql")
    .WithReference(appInsights)
    .WithDataVolume()
    .AddDatabase("sqldb");

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(r =>
    {
        r.WithDataVolume();
    });
var storageDocs = storage.AddBlobs("storagedocs");

var markitdownbash = builder.AddProject<Projects.ChatWYData_MarkDown_MarkitdownBashApi>("markitdownbash")
    .WithExternalHttpEndpoints();

var markdowncsnakes = builder.AddProject<Projects.ChatWYData_MarkDown_MarkitdownCSnakes>("markdowncsnakes")
    .WithExternalHttpEndpoints();

var markdown = builder.AddProject<Projects.ChatWYData_MarkdownApi>("markdown")
    .WithReference(appInsights)
    .WithReference(aoai)
    .WaitFor(aoai)
    .WithReference(markitdownbash)
    .WaitFor(markitdownbash)
    .WithReference(markdowncsnakes)
    .WaitFor(markdowncsnakes)
    .WithExternalHttpEndpoints();

var vectorstore = builder.AddProject<Projects.ChatWYData_VectorStoreAzureAISearch>("vectorstore")
    .WithReference(appInsights)
    .WithReference(aoai)
    .WaitFor(aoai)
    .WithReference(azureaisearch)
    .WaitFor(azureaisearch)
    .WithReference(sqldb)
    .WaitFor(sqldb)
    .WithExternalHttpEndpoints();

var description = builder.AddProject<Projects.ChatWYData_DescriptionApi>("description")
    .WithReference(appInsights)
    .WithReference(aoai)
    .WaitFor(aoai)
    .WithExternalHttpEndpoints();

var documentsApi = builder.AddProject<Projects.ChatWYData_DocumentsApi>("documentapi")
    .WithReference(appInsights)
    .WithReference(sqldb)
    .WaitFor(sqldb)
    .WithReference(aoai)
    .WaitFor(aoai)
    .WithReference(storageDocs)
    .WaitFor(storageDocs)
    .WithReference(markdown)
    .WaitFor(markdown)
    .WithReference(vectorstore)
    .WaitFor(vectorstore)
    .WithReference(description)
    .WaitFor(description);

var docworker = builder.AddProject<Projects.ChatWYData_Workers_DocProcessor>("docworker")
    .WithReference(documentsApi)
    .WaitFor(documentsApi);

var docsmngr = builder.AddProject<Projects.ChatWYData_DocsMngr>("uidocsmngr")
    .WithReference(appInsights)
    .WithReference(documentsApi)
    .WaitFor(documentsApi)
    .WithReference(docworker)
    .WaitFor(docworker)
    .WithExternalHttpEndpoints();

var chatapp = builder.AddProject<Projects.ChatWYData_ChatApp>("uichatapp")
    .WithReference(appInsights)
    .WithReference(documentsApi)
    .WaitFor(documentsApi)
    .WithReference(vectorstore)
    .WaitFor(vectorstore)
    .WithExternalHttpEndpoints();

builder.Build().Run();

//// =======================================================================  
//// DEV ENV without creating services in Azure
//// ======================================================================= 
//var sqldb = builder.AddSqlServer("sql")
//    .WithDataVolume()
//    .AddDatabase("sqldb");

//var storage = builder.AddAzureStorage("storage")
//    .RunAsEmulator(r =>
//    {
//        r.WithDataVolume();
//    });
//var storageDocs = storage.AddBlobs("storagedocs");

//var markitdownbash = builder.AddProject<Projects.ChatWYData_MarkDown_MarkitdownBashApi>("markitdownbash")
//    .WithExternalHttpEndpoints();

//var markdowncsnakes = builder.AddProject<Projects.ChatWYData_MarkDown_MarkitdownCSnakes>("markdowncsnakes")
//    .WithExternalHttpEndpoints();

//var markdown = builder.AddProject<Projects.ChatWYData_MarkdownApi>("markdown")
//    .WithReference(markdowncsnakes)
//    .WaitFor(markdowncsnakes)
//    .WithReference(markitdownbash)
//    .WaitFor(markitdownbash)
//    .WithExternalHttpEndpoints();

//var vectorstore = builder.AddProject<Projects.ChatWYData_VectorStoreAzureAISearch>("vectorstore")
//    .WithReference(sqldb)
//    .WaitFor(sqldb)
//    .WithExternalHttpEndpoints();

//var description = builder.AddProject<Projects.ChatWYData_DescriptionApi>("description")
//    .WithExternalHttpEndpoints();

//var documentsApi = builder.AddProject<Projects.ChatWYData_DocumentsApi>("documentapi")
//    .WithReference(sqldb)
//    .WaitFor(sqldb)
//    .WithReference(storageDocs)
//    .WaitFor(storageDocs)
//    .WithReference(markdown)
//    .WaitFor(markdown)
//    .WithReference(vectorstore)
//    .WaitFor(vectorstore)
//    .WithReference(description)
//    .WaitFor(description);

//var docworker = builder.AddProject<Projects.ChatWYData_Workers_DocProcessor>("docworker")
//    .WithReference(documentsApi)
//    .WaitFor(documentsApi);

//var docsmngr = builder.AddProject<Projects.ChatWYData_DocsMngr>("uidocsmngr")
//    .WithReference(documentsApi)
//    .WaitFor(documentsApi)
//    .WithReference(docworker)
//    .WaitFor(docworker)
//    .WithExternalHttpEndpoints();

//var chatapp = builder.AddProject<Projects.ChatWYData_ChatApp>("uichatapp")
//    .WithReference(documentsApi)
//    .WaitFor(documentsApi)
//    .WithReference(vectorstore)
//    .WaitFor(vectorstore)
//    .WithExternalHttpEndpoints();

//builder.Build().Run();