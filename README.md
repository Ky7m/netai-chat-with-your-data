[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![Twitter: elbruno](https://img.shields.io/twitter/follow/elbruno.svg?style=social)](https://twitter.com/elbruno)
![GitHub: elbruno](https://img.shields.io/github/followers/elbruno?style=social)

# Project Name

**Chat with your Data** is a reference .NET application implementing an Chat application based on documents content with search features using **Semantic Search with Vector Database and Azure AI Search**.

- [Features](#features)
- [Architecture diagram](#architecture-diagram)
- [Getting started](#getting-started)
- [Deploying to Azure](#deploying)
- Run solution
  - [Run locally](#run-locally)
  - [Run the solution](#run-the-solution)
  - [.NET Aspire Azure Resources creation](#net-aspire-azure-resources-creation)- 
  - [Analyze the Vector Store in Azure AI Search](#analyze-the-vector-store-in-azure-ai-search)
  - [Local dev using existing services](#local-development-using-an-existing-services)
  - [Telemetry with .NET Aspire and Azure Application Insights](#telemetry-with-net-aspire-and-azure-application-insights)
- [Resources](#resources)
- [Video Recordings](#video-recordings)
- [Guidance](#guidance)
  - [Costs](#costs)
  - [Security Guidelines](#security-guidelines)
- [Resources](#resources)

## Features

**GitHub CodeSpaces:** This project is designed to be opened in GitHub Codespaces as an easy way for anyone to deploy the solution entirely in the browser.

This is the Chat Application running answering questions based on documents content:

![Chat Aplication running](./images/10ChatApp.png)

This is the  Document Manager application. this application serves as an example on how to add or update new documents to the document database.

![Document Managment Aplication running ](./images/14DocMngr.png)

The Aspire Dashboard to check the running services:

![Aspire Dashboard to check the running services](./images/16AspireDashboard.png)

The Azure Resource Group with all the deployed services:

![Azure Resource Group with all the deployed services](./images/18AzureResourceGroup.png)

The Application Insights Application Map view of the solution:

![Application Insights Application Map view of the solution](./images/20AppInsMap.png)

## Architecture diagram

WIP ![Architecture diagram](./images/30Diagram.png)

## Getting Started

The solution is in the `./src` folder, the main solution is **[Chat-With-Y-Data.sln](./src/Chat-With-Y-Data.sln)**.

## Deploying

Once you've opened the project in [Codespaces](#github-codespaces), or [locally](#run-locally), you can deploy it to Azure.

From a Terminal window, open the folder with the clone of this repo and run the following commands.

1. Login to Azure:

    ```shell
    azd auth login
    ```

1. Provision and deploy all the resources:

    ```shell
    azd up
    ```

    It will prompt you to provide an `azd` environment name (like "**chat=with-your-data**"), select a subscription from your Azure account, and select a [location where Azure AI Search and the OpenAI models gpt-4o-mini and ADA-002 are available](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all) (like "eastus2").

1. When `azd` has finished deploying, you'll see the list of resources created in Azure and a set of URIs in the command output.

1. Visit the **uichatapp** URI, and you should see the **Chat application**! 🎉

1. Visit the **uidocsmngr** URI, and you should see the **Document Manager Application**! 🎉

1. This is an example of the command output:

  ![Deploy Azure Complete](./images/25ConsoleOutputdeploy.png)

1. **Coming Soon!** You can check this video with a 5 minutes overview of the deploy process from codespaces: [Deploy Your **Chat with your Data** to Azure in Minutes!]().

***Note:** The deploy files are located in the `./src/ChatWYData.AppHost/infra/` folder. They are generated by the `Aspire AppHost` project.*

### GitHub CodeSpaces

- Create a new  Codespace using the `Code` button at the top of the repository.

  ![create Codespace](./images/25CreateCodeSpaces.png)

- The Codespace creation process can take a couple of minutes.

- Once the Codespace is loaded, it should have all the necessary requirements to deploy the solution.

### Run Locally

To run the project locally, you'll need to make sure the following tools are installed:

- [.NET 9](https://dotnet.microsoft.com/downloads/)
- [Git](https://git-scm.com/downloads)
- [Azure Developer CLI (azd)](https://aka.ms/install-azd)
- [Visual Studio Code](https://code.visualstudio.com/Download) or [Visual Studio](https://visualstudio.microsoft.com/downloads/)
  - If using Visual Studio Code, install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- .NET Aspire workload:
    Installed with the [Visual Studio installer](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire) or the [.NET CLI workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#install-net-aspire).
- An OCI compliant container runtime, such as:
  - [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/).

### Run the solution

Follow these steps to run the project, locally or in CodeSpaces:

- Navigate to the Aspire Host folder project using the command:

  ```bash
  cd ./src/eShopAppHost/
  ```

- If you are running the project in Codespaces, you need to run this command:

  ```bash
  dotnet dev-certs https --trust
  ```

- By default the AppHost project creates the necessary resources on Azure. Check the **[.NET Aspire Azure Resources creation](#net-aspire-azure-resources-creation)** section to learn how to configure the project to create Azure resources.

- Run the project:

  ```bash
  dotnet run
  ````

Check the [Video Resources](#resources) for a step-by-step on how to run this project.

## .NET Aspire Azure Resources creation

When utilizing Azure resources in your local development environment, you need to:

- Authenticate to the Azure Tenant where the resources will be created. Run the following command to connect with your Azure tenant:

  ```bash
  az login 
  ```
- Provide the necessary Configuration values are specified under the Azure section in the `eShopAppHost` project:

  - CredentialSource: Delegates to the [AzureCliCredential](https://learn.microsoft.com/dotnet/api/azure.identity.azureclicredential).
  - SubscriptionId: The Azure subscription ID.
  - AllowResourceGroupCreation: A boolean value that indicates whether to create a new resource group.
  - ResourceGroup: The name of the resource group to use.
  - Location: The Azure region to use.

Consider the following example for the *appsettings.json* file in the eShopAppHost project configuration:

```json
{
  "Azure": {
    "CredentialSource": "AzureCli",
    "SubscriptionId": "<Your subscription id>",
    "AllowResourceGroupCreation": true,
    "ResourceGroup": "<Valid resource group name>",
    "Location": "<Valid Azure location>"
  }
}
```

Check [.NET Aspire Azure hosting integrations](https://learn.microsoft.com/en-us/dotnet/aspire/azure/local-provisioning#net-aspire-azure-hosting-integrations) for more information on how .NET Aspire create the necessary cloud resources for local development.

### Analyze the Vector Store in Azure AI Search

To create and fill with data the Vector Store in Azure AI Search, you need to perform a Semantic Search in the application.

![perform a Semantic Search in the application](./images/40SemantiSearchResultinBlazorApp.png)

The first search will fill the Vector Store with all the store products.

![The first search will fill the Vector Store with all the store products.](./images/421stSemanticSearchInitTheVectorStore.png)

When you perform a new Semantic Search, the elapsed time will be must faster than the 1st one.

![the elapsed time will be must faster than the 1st one](./images/442ndSemantiSearchtimes.png)

And the trace will show:

- The search request from `store` to `products`
- `products` calling the Azure OpenAI embedding model to generate an embedding with the search criteria
- `products` calling the Azure AI Search to query the vector store using the search criteria
- `products` calling the Azure OpenAI chat model to generate a user friendly response

![Complete trace for a standard semantic search](./images/46TraceForStandardSemanticSearch.png)

You can also open the Azure AI Search resource in the Azure portal, and check the created index **products** with the data and fields.

![Azure AI Search resource in the Azure portal, and check the created index **products** with the data and fields](./images/48AzureAISearchIndex.png)

### Local development using an existing services

In order to use existing **Azure AI Search Services** and existing **Azure OpenAI models**, like gpt-4o-mini and text-embedding-ada-002, you need to make changes in 2 projects:

#### Aspire AppHost

Open the `program.cs` in `.\src\eShopAppHost\`, and comment the main aspire lines, and uncomment the lines to only create and run the sqldb, the api project and the front end.

![Comment the aspire lines and uncomment the last lines](./images/30RunUsingExistingServices.png)

#### Products

Edit and define specific connection strings in the `Products` project.

Add a user secret running the commands:

```bash
cd src/Products
dotnet user-secrets set "ConnectionStrings:openaidev" "Endpoint=https://<endpoint>.openai.azure.com/;Key=<Azure OpenAI Service key>;"
dotnet user-secrets set "ConnectionStrings:azureaisearchdev" "Endpoint=https://<endpoint>.search.windows.net/;Key=<Azure AI Search key>;"
```

Update the code to use connection strings which names are `azureaisearchdev` and `openaidev`. Change this:

```csharp
// To reuse existing Azure AI Search resources, this to "azureaisearchdev", and check the documentation on how to reuse the resources
var azureAiSearchName = "azureaisearch";
builder.AddAzureSearchClient(azureAiSearchName);

// To reuse existing Azure OpenAI resources, this to "openaidev", and check the documentation on how to reuse the resources
var azureOpenAiClientName = "openai";
builder.AddAzureOpenAIClient(azureOpenAiClientName);
```

to this:

```csharp
// To reuse existing Azure AI Search resources, this to "azureaisearchdev", and check the documentation on how to reuse the resources
var azureAiSearchName = "azureaisearchdev";
builder.AddAzureSearchClient(azureAiSearchName);

// To reuse existing Azure OpenAI resources, this to "openaidev", and check the documentation on how to reuse the resources
var azureOpenAiClientName = "openaidev";
builder.AddAzureOpenAIClient(azureOpenAiClientName);
```

### Telemetry with .NET Aspire and Azure Application Insights

The eShopLite solution leverages the Aspire Dashboard and Azure Application Insights to provide comprehensive telemetry and monitoring capabilities

The **.NET Aspire Dashboard** offers a centralized view of the application's performance, health, and usage metrics. It integrates seamlessly with the Azure OpenAI services, allowing developers to monitor the performance of the `gpt-4o-mini` and `text-embedding-ada-002` models. The dashboard provides real-time insights into the application's behavior, helping to identify and resolve issues quickly.

![Aspire Dashboard](./images/50AspireDashboard.png)

**Azure Application Insights** complements the Aspire Dashboard by offering deep diagnostic capabilities and advanced analytics. It collects detailed telemetry data, including request rates, response times, and failure rates, enabling developers to understand how the application is performing under different conditions. Application Insights also provides powerful querying and visualization tools, making it easier to analyze trends and detect anomalies. 

![Azure Application Insights](./images/52AppInsightsDashboard.png)

By combining the Aspire Dashboard with Azure Application Insights, the eShopLite solution ensures robust monitoring and diagnostics, enhancing the overall reliability and performance of the application.

## Guidance

### Costs

For **Azure OpenAI Services**, pricing varies per region and usage, so it isn't possible to predict exact costs for your usage. Same applies to **Azure AI Search**.
The majority of the Azure resources used in this infrastructure are on usage-based pricing tiers.
However, Azure Container Registry has a fixed cost per registry per day.

You can try the [Azure pricing calculator](https://azure.com/e/2176802ea14941e4959eae8ad335aeb5) for the resources:

- Azure OpenAI Service: S0 tier, gpt-4o-mini and text-embedding-ada-002 models. Pricing is based on token count. [Pricing](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)
- Azure Container App: Consumption tier with 0.5 CPU, 1GiB memory/storage. Pricing is based on resource allocation, and each month allows for a certain amount of free usage. [Pricing](https://azure.microsoft.com/pricing/details/container-apps/)
- Azure Container Registry: Basic tier. [Pricing](https://azure.microsoft.com/pricing/details/container-registry/)
- Log analytics: Pay-as-you-go tier. Costs based on data ingested. [Pricing](https://azure.microsoft.com/pricing/details/monitor/)
- Azure AI Search: [Basic tier](https://azure.microsoft.com/pricing/details/search/). You can edit the bicep files to change for the free tier.
- Azure Application Insights pricing is based on a Pay-As-You-Go model. [Pricing](https://learn.microsoft.com/azure/azure-monitor/logs/cost-logs).

⚠️ To avoid unnecessary costs, remember to take down your app if it's no longer in use, either by deleting the resource group in the Portal or running `azd down`.

### Security Guidelines

Samples in this templates uses Azure OpenAI Services with ApiKey and [Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for authenticating to the Azure OpenAI service.

The Main Sample uses Managed Identity](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview) for authenticating to the Azure OpenAI service.

Additionally, we have added a [GitHub Action](https://github.com/microsoft/security-devops-action) that scans the infrastructure-as-code files and generates a report containing any detected issues. To ensure continued best practices in your own repository, we recommend that anyone creating solutions based on our templates ensure that the [Github secret scanning](https://docs.github.com/code-security/secret-scanning/about-secret-scanning) setting is enabled.

You may want to consider additional security measures, such as:

- Protecting the Azure Container Apps instance with a [firewall](https://learn.microsoft.com/azure/container-apps/waf-app-gateway) and/or [Virtual Network](https://learn.microsoft.com/azure/container-apps/networking?tabs=workload-profiles-env%2Cazure-cli).

## Resources

(Any additional resources or related projects)

- Link to supporting information
- Link to similar sample
- ...
