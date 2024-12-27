using ChatWYData.DataEntities;
using ChatWYData.DocumentsApi.Models;

public static class DbInitializer
{
    public static void Initialize(Context context)
    {
        if (context.Document.Any())
            return;

        //var documents = new List<Document>();
        //// for 1 to 10
        //for (var i = 1; i <= 10; i++)
        //{
        //    var doc = new Document { FileName = $"doc {i}", FileDescription = $"doc {i} desc", FileContentType = "image/jpeg", FileBlobUri = "" };
        //    documents.Add(doc);
        //}
        //context.AddRange(documents);
        context.SaveChanges();
    }
}
