using ChatWYData.DataEntities;
using ChatWYData.SearchEntities;
using System.Net.Http.Json;

namespace ChatWYData.ApiServices;

public class VectorStoreApiService
{
    HttpClient httpClient;

    public VectorStoreApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<Document?> UpsertAsync(Document document, CancellationToken cancellationToken = default)
    {
        Document? docResponse = null;

        var response = await httpClient.PostAsJsonAsync<Document>("/upsert", document, cancellationToken);
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            docResponse = await response.Content.ReadFromJsonAsync<Document>(cancellationToken: cancellationToken);
        }

        return docResponse;
    }
    public async Task<SearchResponse?> SearchAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        SearchResponse? searchResponse = null;

        var response = await httpClient.PostAsJsonAsync("/search", searchRequest, cancellationToken);
        if (response != null)
        {
            response.EnsureSuccessStatusCode();
            searchResponse = await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken: cancellationToken);
        }

        return searchResponse;
    }
}
