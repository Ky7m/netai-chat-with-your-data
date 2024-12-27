namespace ChatWYData.ApiServices;

public class DocProcessorWorkerApiService
{
    HttpClient httpClient;

    public DocProcessorWorkerApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await PerformGetRequest("/health");
    }

    public async Task<string> StopAsync(CancellationToken cancellationToken = default)
    {
        return await PerformGetRequest("/stop");
    }

    public async Task<string> StartAsync(CancellationToken cancellationToken = default)
    {
        return await PerformGetRequest("/start");
    }

    public async Task<string> ProcessDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await PerformGetRequest("/process");
    }

    private async Task<string> PerformGetRequest(string requestUri, CancellationToken cancellationToken = default)
    {
        string serviceResponse = string.Empty;
        var response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            serviceResponse = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
        }

        return serviceResponse;
    }

}
