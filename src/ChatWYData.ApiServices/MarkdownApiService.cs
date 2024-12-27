using ChatWYData.FileEntities;
using System.Net.Http.Json;

namespace ChatWYData.ApiServices;

public class MarkdownApiService
{
    HttpClient httpClient;

    public MarkdownApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<FileUploadResponse?> ConvertAsync(FileUploadRequest fuRequest, CancellationToken cancellationToken = default)
    {
        FileUploadResponse? fuResponse = null;

        var response = await httpClient.PostAsJsonAsync<FileUploadRequest>("/generatemd", fuRequest, cancellationToken);
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            fuResponse = await response.Content.ReadFromJsonAsync<FileUploadResponse>(cancellationToken: cancellationToken);
        }

        return fuResponse;
    }

}
