using ChatWYData.FileEntities;
using System.Net.Http.Json;

namespace ChatWYData.ApiServices;

public class MarkidownCSnakesApiService
{
    HttpClient httpClient;

    public MarkidownCSnakesApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<string?> ConvertAsync(FileUploadRequest fuRequest, CancellationToken cancellationToken = default)
    {
        string mdResponse = "";

        var response = await httpClient.PostAsJsonAsync<FileUploadRequest>("/convert", fuRequest, cancellationToken);
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            mdResponse = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
        }

        return mdResponse;
    }

}
