using ChatWYData.DataEntities;
using System.Net.Http.Json;

namespace ChatWYData.ApiServices;

public class DescriptionApiService
{
    HttpClient httpClient;

    public DescriptionApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<Document> GenerateDescAsync(Document document, CancellationToken cancellationToken = default)
    {
        Document? documentResponse = null;
        var response = await httpClient.PostAsJsonAsync($"/generate_desc", document, cancellationToken);

        if (response != null) {
            response.EnsureSuccessStatusCode();
            documentResponse = await response.Content.ReadFromJsonAsync<Document>(cancellationToken: cancellationToken);
        }

        return documentResponse  ?? new Document();
    }


}
