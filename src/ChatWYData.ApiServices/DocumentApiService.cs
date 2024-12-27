using ChatWYData.FileEntities;
using ChatWYData.DataEntities;
using System.Net.Http.Json;

namespace ChatWYData.ApiServices;

public class DocumentApiService
{
    HttpClient httpClient;

    public DocumentApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<List<Document>> GetDocumentsAsync(CancellationToken cancellationToken = default)
    {
        List<Document>? documents = null;
        var response = await httpClient.GetAsync("/docs/getall");
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            documents = await response.Content.ReadFromJsonAsync<List<Document>>(cancellationToken: cancellationToken);
        }
        return documents ?? new List<Document>();
    }

    public async Task<Document> GetDocumentAsync(string fileName, CancellationToken cancellationToken = default)
    {
        Document? document = null;
        var response = await httpClient.GetAsync($"/docs/get/{fileName}");

        if (response != null) {
            response.EnsureSuccessStatusCode();
            document = await response.Content.ReadFromJsonAsync<Document>(cancellationToken: cancellationToken);
        }

        return document ?? new Document();
    }

    public async Task<string> GetDocumentUriAsync(string fileName, CancellationToken cancellationToken = default)
    {
        string? documentUri = null;
        var response = await httpClient.GetAsync($"/docs/getDocUri/{fileName}");

        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            documentUri = await response.Content.ReadFromJsonAsync<string>(cancellationToken: cancellationToken);
        }

        return documentUri ?? string.Empty;
    }


    public async Task<FileBatchUploadResponse?> FileBatchUploadDocAsync(FileUploadRequest fuRequest, CancellationToken cancellationToken = default)
    {
        FileBatchUploadResponse? fuResponse = null;
        var response = await httpClient.PostAsJsonAsync("/filebatch/upload", fuRequest, cancellationToken);
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            fuResponse = await response.Content.ReadFromJsonAsync<FileBatchUploadResponse>(cancellationToken: cancellationToken);
        }
        return fuResponse;
    }

    public async Task<FileBatchUploadResponse?> FileBatchProcessDocAsync(FileUploadRequest fuRequest, CancellationToken cancellationToken = default)
    {
        FileBatchUploadResponse? fuResponse = null;
        var response = await httpClient.PostAsJsonAsync("/filebatch/processdoc", fuRequest, cancellationToken);
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            fuResponse = await response.Content.ReadFromJsonAsync<FileBatchUploadResponse>(cancellationToken: cancellationToken);
        }
        return fuResponse;
    }

    public async Task<List<FileProcessBatch>> GetFileBatchUploadsAsync(CancellationToken cancellationToken = default)
    {
        List<FileProcessBatch>? fileBatchUploads = null;
        var response = await httpClient.GetAsync("/filebatch/getall");
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            fileBatchUploads = await response.Content.ReadFromJsonAsync<List<FileProcessBatch>>(cancellationToken: cancellationToken);
        }
        return fileBatchUploads ?? [];
    }


    public async Task<List<FileProcessBatch>> GetFileBatchUploadsToProcessAsync(CancellationToken cancellationToken = default)
    {
        List<FileProcessBatch>? fileBatchUploads = null;
        var response = await httpClient.GetAsync($"/filebatch/getallforprocess");
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            fileBatchUploads = await response.Content.ReadFromJsonAsync<List<FileProcessBatch>>(cancellationToken: cancellationToken);
        }
        return fileBatchUploads ?? [];
    }

    public async Task<List<FileProcessBatch>> GetFileBatchUploadsToProcessByProcessedAndIgnoreAsync(bool processed, bool ignore, CancellationToken cancellationToken = default)
    {
        List<FileProcessBatch>? fileBatchUploads = null;
        var response = await httpClient.GetAsync($"/filebatch/getallbyProcessedAndIgnore/{processed}/{ignore}");
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            fileBatchUploads = await response.Content.ReadFromJsonAsync<List<FileProcessBatch>>(cancellationToken: cancellationToken);
        }
        return fileBatchUploads ?? [];
    }

    public async Task<List<FileProcessBatch>> GetFileBatchUploadsToProcessByProcessed(bool processed, CancellationToken cancellationToken = default)
    {
        List<FileProcessBatch>? fileBatchUploads = null;
        var response = await httpClient.GetAsync($"/filebatch/getallbyProcessed/{processed}");
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            fileBatchUploads = await response.Content.ReadFromJsonAsync<List<FileProcessBatch>>(cancellationToken: cancellationToken);
        }
        return fileBatchUploads ?? [];
    }

    public async Task<bool> IgnoreSingleDocumentAsync(string fileName, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var response = await httpClient.GetAsync($"/filebatch/updateignore/{fileName}");
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
            result = bool.TryParse(responseString, out bool parsedResult) && parsedResult;
        }
        return result;
    }

}
