using ChatWYData.DataEntities;

namespace ChatWYData.SearchEntities;

public class SearchResponse
{
    public string ResponseMessage { get; set; }
    public List<Document> Documents { get; set; }
}
