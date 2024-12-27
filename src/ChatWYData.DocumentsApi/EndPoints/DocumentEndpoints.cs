using ChatWYData.DocumentsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatWYData.DocumentsApi.EndPoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/docs/getall", async (Context db) =>
        {
            var docs = await db.Document.ToListAsync();
            return docs;
        });

        routes.MapGet("/docs/get/{fileName}", async (string fileName, Context db) =>
        {
            return await db.Document.FirstOrDefaultAsync(d => d.FileName == fileName);
        });

        routes.MapGet("/docs/getDocUri/{fileName}", async (string fileName, Context db) =>
        {
            var doc = await db.Document.FirstOrDefaultAsync(d => d.FileName == fileName);
            return doc?.FileBlobUri;
        });
    }
}

