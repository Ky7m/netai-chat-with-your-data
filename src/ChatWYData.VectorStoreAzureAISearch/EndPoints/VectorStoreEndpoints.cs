using Azure;
using ChatWYData.DataEntities;
using ChatWYData.FileEntities;
using ChatWYData.SearchEntities;
using ChatWYData.VectorEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.Identity.Client;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text;

namespace ChatWYData.VectorStoreAzureAISearch.EndPoints;

public static class VectorStoreEndpoints
{

    public static void MapVectorStoreEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/upsert", async (HttpContext httpContext, DataEntities.Document document, ILogger<Program> logger, EmbeddingClient? embeddingClient, ChatClient? chatClient, IVectorStoreRecordCollection<string, DocumentVector> documentCollection, Context db) =>
        {
            logger.LogInformation($"Processing document: {document.FileName}");

            // get the sanitized file name key
            var fileNameKey = document.FileName.Replace(".", "")
            .Replace("_", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace(" ", "")
            .Replace("-", "");

            // generate vector information
            var documentInfo = document.FileMarkDown;
            var chunkSize = 4096;
            var chunks = Enumerable.Range(0, (documentInfo.Length + chunkSize - 1) / chunkSize)
                                   .Select(i => documentInfo.Substring(i * chunkSize, Math.Min(chunkSize, documentInfo.Length - i * chunkSize)))
                                   .ToList();
            logger.LogInformation($">> Chunks: {chunks.Count}");
            await UpdateFileProcessBatchStatus(db, document.FileName, $">> Chunks: {chunks.Count}");
            // save the document into chunks
            var i = 0;
            foreach (var chunk in chunks)
            {
                i++;
                var newFileNameKey = $"{fileNameKey}_{i}";
                try
                {
                    var chunckEmbeddings = await embeddingClient.GenerateEmbeddingAsync(chunk);
                    var documentVector = new DocumentVector
                    {
                        FileNameKey = newFileNameKey,
                        FileName = document.FileName,
                        FileDescription = document.FileDescription,
                        FileMarkDown = chunk, //document.FileMarkDown,
                        FileContentType = document.FileContentType,
                        FileBlobUri = document.FileBlobUri,
                        Vector = chunckEmbeddings.Value.ToFloats()
                    };

                    var recordId = await documentCollection.UpsertAsync(documentVector);
                    logger.LogInformation($"Document added to memory: {documentVector.FileNameKey} - RecordId: {recordId}");
                    await UpdateFileProcessBatchStatus(db, document.FileName, $">> Memory added : {documentVector.FileNameKey}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing file {newFileNameKey} - chunk: {i}");
                }
            }
            logger.LogInformation($"Document added to memory: {document.FileName} in [{i}] chunks.");
            await UpdateFileProcessBatchStatus(db, document.FileName, $"Document added to memory: {document.FileName} in [{i}] chunks.");

            document.Chunks = i;

            return document;
        });

        routes.MapPost("/search", async (
            HttpContext httpContext, 
            SearchRequest searchRequest, 
            ILogger<Program> logger, 
            EmbeddingClient? embeddingClient, 
            ChatClient? chatClient, 
            IVectorStoreRecordCollection<string, 
            DocumentVector> documentCollection) =>
        {
            logger.LogInformation($"Searching documents for: {searchRequest.Query}, Max Results: {searchRequest.MaxResults}");

            var response = new SearchResponse
            {
                ResponseMessage = "",
                Documents = []
            };

            try
            {
                // prepare query for Azure AI Search
                var azureAISearchQuery = await GenerateQueryForAzureAISearch(searchRequest, logger, chatClient);
                logger.LogInformation($"Query for Azure AI Search: [{azureAISearchQuery}]");

                var result = await embeddingClient.GenerateEmbeddingAsync(azureAISearchQuery);
                var vectorSearchQuery = result.Value.ToFloats();

                var searchOptions = new VectorSearchOptions()
                {
                    Top = searchRequest.MaxResults,
                    VectorPropertyName = "Vector"
                };

                // search the vector database for the most similar product        
                var searchResults = await documentCollection.VectorizedSearchAsync(vectorSearchQuery, searchOptions);
                var documentsFound = new StringBuilder();
                var i = 0;
                await foreach (var searchItem in searchResults.Results)
                {
                    if (searchItem.Score > (searchRequest.MinScore / 100.0))
                    {
                        i++;
                        var doc = new Document
                        {
                            FileName = searchItem.Record.FileName,
                            FileDescription = searchItem.Record.FileDescription,
                            FileMarkDown = searchItem.Record.FileMarkDown,
                            FileContentType = searchItem.Record.FileContentType,
                            FileBlobUri = searchItem.Record.FileBlobUri
                        };

                        // add doc
                        response.Documents.Add(doc);
                        logger.LogInformation($"Document found: {doc.FileName} with score: {searchItem.Score}");
                        documentsFound.AppendLine($"Found Document FileName: {doc.FileName} - Index: {i} - FileUri: {doc.FileBlobUri} - Document Markdown: {doc.FileMarkDown}");
                    }
                }

                // validate if we found any documents
                if (i > 0)
                {
                    // let's improve the response message
                    var prompt = @$"You are an intelligent assistant helping clients with their search about documents. Generate a catchy and friendly response to the user question using the following information:
- User Question: [{searchRequest.Query}]
- Number of Documents found: {i}
- Documents Information : {documentsFound.ToString()}
Include the found documents information in the response to the user question. 
Do not include the documents URI to be displayed in the response.";
                    
                    var chatResponse = await chatClient.CompleteChatAsync(prompt);
                    var responseText = chatResponse.Value.Content[0].Text;
                    response.ResponseMessage = responseText;

                }
                else
                {
                    logger.LogInformation("No documents found, running the query directly to the model");
                    var chatResponse = await chatClient.CompleteChatAsync(searchRequest.Query);
                    var responseText = chatResponse.Value.Content[0].Text;
                    response.ResponseMessage = responseText;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error searching documents");
                response.ResponseMessage = "Error searching documents";
            }

            logger.LogInformation($"Search completed. Response: {response.ResponseMessage}");

            return response;
        });
    }

    private static async Task UpdateFileProcessBatchStatus(Context db, string fileName, string statusMessage, bool processed = false)
    {
        var fileProcessBatch = await db.FileProcessBatch.FirstOrDefaultAsync(d => d.FileName == fileName);
        fileProcessBatch.StatusMessage = statusMessage;
        fileProcessBatch.LastModifiedDate = DateTime.UtcNow;
        fileProcessBatch.StepsDescription = $"{fileProcessBatch.LastModifiedDate} - {fileProcessBatch.StatusMessage}{Environment.NewLine}{fileProcessBatch.StepsDescription}";
        fileProcessBatch.Processed = processed;
        db.FileProcessBatch.Update(fileProcessBatch);
        db.SaveChanges();
    }

    private static async Task<string> GenerateQueryForAzureAISearch(SearchRequest searchRequest, ILogger<Program> logger, ChatClient? chatClient)
    {
        var messages = SearchRequest.GetChatMessagesFromSearchMessages(searchRequest.Messages);

        // search the message collection and remove a System Chat Message if exists
        var systemMessage = messages.FirstOrDefault(m => m is SystemChatMessage);
        if (systemMessage != null)
        {
            messages.Remove(systemMessage);
        }

        // add a system message to the collection to process the new Azure AI Search Query
        var prompt = @"Below is a history of the conversation so far, and a new question asked by the user that needs to be answered by searching in a knowledge base.
    You have access to Azure AI Search index with 100's of documents.
    Generate a search query based on the conversation and the new question.
    Do not include cited source filenames and document names e.g info.txt or doc.pdf in the search query terms.
    Do not include any text inside [] or <<>> in the search query terms.
    Do not include any special characters like '+'.
    If the question is not in English, translate the question to English before generating the search query.
    If you cannot generate a search query, return just the number 0.";
        systemMessage = new SystemChatMessage(prompt);
        messages.Add(systemMessage);

        // generate the azure ai search query
        var chatResponse = await chatClient.CompleteChatAsync(messages);
        var responseText = chatResponse.Value.Content[0].Text;
        return responseText;
    }
}

