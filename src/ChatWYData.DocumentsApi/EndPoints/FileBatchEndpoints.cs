using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChatWYData.ApiServices;
using ChatWYData.DataEntities;
using ChatWYData.DocumentsApi.Models;
using ChatWYData.FileEntities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ChatWYData.DocumentsApi.EndPoints;

public static class FileBatchEndpoints
{
    public static void MapFileBatchEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/filebatch/getall", async (Context db) =>
        {
            var docs = await db.FileProcessBatch.ToListAsync();
            return docs;
        });

        routes.MapGet("/filebatch/getallforprocess", async (Context db) =>
        {
            // get all files where field processed and ignore is false, ordered by created date asc    
            var docs = await db.FileProcessBatch.Where(d => d.Processed == false && d.Ignore == false).OrderBy(d => d.CreatedDate).ToListAsync();
            return docs;
        });

        routes.MapGet("/filebatch/getallbyProcessedAndIgnore/{processed}/{ignore}", async (bool processed, bool ignore, Context db) =>
        {
            // get all files where field processed and ignore is false, ordered by created date asc    
            var docs = await db.FileProcessBatch.Where(d => d.Processed == processed && d.Ignore == ignore).OrderBy(d => d.CreatedDate).ToListAsync();
            return docs;
        });

        routes.MapGet("/filebatch/getallbyProcessed/{processed}", async (bool processed, Context db) =>
        {
            // get all files where field processed and ignore is false, ordered by created date asc    
            var docs = await db.FileProcessBatch.Where(d => d.Processed == processed).OrderBy(d => d.CreatedDate).ToListAsync();
            return docs;
        });

        routes.MapGet("/filebatch/updateignore/{fileName}", async (string fileName, Context db) =>
        {
            var res = false;
            // change the ignore field to the opposite value and return the new value
            var doc = await db.FileProcessBatch.FirstOrDefaultAsync(d => d.FileName == fileName);
            if (doc != null)
            {
                doc.Ignore = !doc.Ignore;
                db.FileProcessBatch.Update(doc);
                db.SaveChanges();
                res = doc.Ignore;
            }
            return res;
        });

        routes.MapPost("/filebatch/upload", async (HttpContext httpContext, FileUploadRequest fuRequest, ILogger<Program> logger, BlobServiceClient blobServiceClient, Context db, MarkdownApiService markdownApiService, VectorStoreApiService vectorStoreApiService) =>
        {
            logger.LogInformation($"Processing file: {fuRequest.FileName}");

            var document = new Document
            {
                FileName = fuRequest.FileName,
                FileContentType = fuRequest.FileContentType
            };

            // save doc to blob
            document = await UploadFileToBlobStorage(fuRequest, logger, blobServiceClient, document);

            // add / update the process batch queue
            var fileProcessBatch = await db.FileProcessBatch
            .FirstOrDefaultAsync(d => d.FileName == fuRequest.FileName);

            if (fileProcessBatch is null)
            {
                fileProcessBatch = new FileProcessBatch
                {
                    FileName = fuRequest.FileName,
                    FileBlobUri = document.FileBlobUri,
                    FileContentType = fuRequest.FileContentType,
                    StatusMessage = "Uploaded",
                    Processed = false
                };
                db.FileProcessBatch.Add(fileProcessBatch);
            }
            else
            {
                fileProcessBatch.FileBlobUri = document.FileBlobUri;
                fileProcessBatch.StatusMessage = "Doc updated from upload";
                fileProcessBatch.LastModifiedDate = DateTime.UtcNow;
                fileProcessBatch.Processed = false;
                fileProcessBatch.StepsDescription = string.Empty;
                db.FileProcessBatch.Update(fileProcessBatch);
            }
            db.SaveChanges();

            // return the response
            var filebatchUploadResponse = new FileBatchUploadResponse
            {
                FileName = document.FileName
            };

            logger.LogInformation($"upload done: {filebatchUploadResponse.FileName}");

            return filebatchUploadResponse;
        });

        routes.MapPost("/filebatch/processdoc", async (FileUploadRequest fuRequest, BlobServiceClient blobServiceClient, Context db, MarkdownApiService markdownApiService, VectorStoreApiService vectorStoreApiService, DescriptionApiService descriptionApiService, ILogger<Program> logger) =>
        {
            // Process document steps:
            // 1. Get the file from the blob storage
            // 2. Search the document in the database
            // 3. Get the file Markdown content and file description
            // 4. Add the document to the vector store and get the file description
            // 5. Generate document description
            // 6. Update database with the document information

            logger.LogInformation($"Processing file: {fuRequest.FileName}");

            var filebatchUploadResponse = new FileBatchUploadResponse
            {
                FileName = fuRequest.FileName
            };

            // get the file batch from the database
            var fileProcessBatch = await db.FileProcessBatch.FirstOrDefaultAsync(d => d.FileName == fuRequest.FileName);

            if (fileProcessBatch == null)
            {
                logger.LogError($"File batch not found for file: {fuRequest.FileName}");
                return filebatchUploadResponse;
            }

            await UpdateFileProcessBatchStatus(db, fuRequest, "starting process");

            // get the file byte array content from the fileProcessBatch.FileBlobUri
            var blobClient = blobServiceClient.GetBlobContainerClient("docs");
            var fileBytes = await blobClient.GetBlobClient(fuRequest.FileName).DownloadContentAsync();
            fuRequest.FileBytes = fileBytes.Value.Content.ToArray();

            // . Search the document in the database
            var addDoc = false;
            logger.LogInformation($"Save or update the document in the database");
            var document = await db.Document.FirstOrDefaultAsync(d => d.FileName == fuRequest.FileName);
            if (document == null)
            {
                logger.LogInformation($"Document not found. Creating a new one.");
                document = new Document();
                addDoc = true;
            }
            document.FileName = fileProcessBatch.FileName;
            document.FileContentType = fileProcessBatch.FileContentType;
            document.FileBlobUri = fileProcessBatch.FileBlobUri;

            logger.LogInformation($"DONE Save or update the document in the database");

            await UpdateFileProcessBatchStatus(db, fuRequest, "document saved to database");

            // . Get the file Markdown content and file description
            document = await GenerateMarkdownContent(fuRequest, logger, markdownApiService, document);
            await UpdateFileProcessBatchStatus(db, fuRequest, "document markdown generated");

            // . Add the document to the vector store and get the file description
            document = await UpsertDocumentInVectorStore(logger, vectorStoreApiService, document);
            await UpdateFileProcessBatchStatus(db, fuRequest, "vector store document information saved");

            // . Generate doc description
            document = await GenerateDocumentDescription(logger, descriptionApiService, document);
            await UpdateFileProcessBatchStatus(db, fuRequest, "document description generated");

            // . Update database with the document information
            SaveOrUpdateDocumentInDatabase(db, addDoc, document);
            await UpdateFileProcessBatchStatus(db, fuRequest, "document updated in the database");

            await UpdateFileProcessBatchStatus(db, fuRequest, "done", true);

            return filebatchUploadResponse;
        });
    }

    private static async Task UpdateFileProcessBatchStatus(Context db, FileUploadRequest fuRequest, string statusMessage, bool processed = false)
    {
        var fileProcessBatch = await db.FileProcessBatch.FirstOrDefaultAsync(d => d.FileName == fuRequest.FileName);
        fileProcessBatch.StatusMessage = statusMessage;
        fileProcessBatch.LastModifiedDate = DateTime.UtcNow;
        fileProcessBatch.StepsDescription = $"{fileProcessBatch.LastModifiedDate} - {fileProcessBatch.StatusMessage}{Environment.NewLine}{fileProcessBatch.StepsDescription}";
        fileProcessBatch.Processed = processed;
        db.FileProcessBatch.Update(fileProcessBatch);
        db.SaveChanges();
    }

    private static async Task<Document> UpsertDocumentInVectorStore(ILogger<Program> logger, VectorStoreApiService vectorStoreApiService, Document? document)
    {
        logger.LogInformation($"Add the document to the vector store and get the file description");
        var vectorDocument = await vectorStoreApiService.UpsertAsync(document);
        document.Chunks = vectorDocument.Chunks;
        logger.LogInformation($"DONE Add the document to the vector store. Chunks: {document.Chunks}");
        return document;
    }

    private static async Task<Document> GenerateDocumentDescription(ILogger<Program> logger, DescriptionApiService descriptionApiService, Document? document)
    {
        logger.LogInformation($"Start generate document description.");
        var descDocument = await descriptionApiService.GenerateDescAsync(document);
        document.FileDescription = descDocument.FileDescription;
        logger.LogInformation($"DONE generate document description.");
        return document;
    }

    private static async Task<Document> GenerateMarkdownContent(FileUploadRequest fuRequest, ILogger<Program> logger, MarkdownApiService markdownApiService, Document? document)
    {
        logger.LogInformation($"Get the file Markdown content");

        var markDownResponse = await markdownApiService.ConvertAsync(fuRequest);
        document.FileMarkDown = markDownResponse.MarkDown;
        logger.LogInformation($"DONE Markdown: {markDownResponse.MarkDown}");

        return document;
    }

    private static async Task<DataEntities.Document> UploadFileToBlobStorage(FileUploadRequest fuRequest, ILogger<Program> logger, BlobServiceClient blobServiceClient, Document? document)
    {
        // upload the file to the blob storage
        logger.LogInformation($"Getting Blob client: [docs]");
        var blobClient = blobServiceClient.GetBlobContainerClient("docs");
        await blobClient.CreateIfNotExistsAsync();

        // check if the file exists
        var blobExists = await blobClient.GetBlobClient(fuRequest.FileName).ExistsAsync();
        if (blobExists.Value)
        {
            logger.LogInformation($"File [{fuRequest.FileName}] already exists. Deleting it.");
            DeleteSnapshotsOption options = DeleteSnapshotsOption.IncludeSnapshots;
            await blobClient.GetBlobClient(fuRequest.FileName).DeleteIfExistsAsync(snapshotsOption: options);
        }
        var fileContent = new MemoryStream(fuRequest.FileBytes);
        var uploadResponse = await blobClient.UploadBlobAsync(fuRequest.FileName, fileContent);
        var uploadedDocUrl = blobClient.GetBlobClient(fuRequest.FileName).Uri;

        // update doc information with the blob data
        document.FileBlobUri = uploadedDocUrl.ToString();
        logger.LogInformation($"Upload file done. File Blob Uri: {document.FileBlobUri}");
        return document;
    }

    private static void SaveOrUpdateDocumentInDatabase(Context db, bool addDoc, Document? document)
    {
        if (addDoc)
        {
            db.Document.Add(document);
        }
        else
        {
            document.UpdatedAt = DateTime.UtcNow;
            db.Document.Update(document);
        }
        db.SaveChanges();
    }
}

