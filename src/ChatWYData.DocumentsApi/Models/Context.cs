using ChatWYData.DataEntities;
using Microsoft.EntityFrameworkCore;

namespace ChatWYData.DocumentsApi.Models;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Document> Document => Set<Document>();
    public DbSet<FileProcessBatch> FileProcessBatch => Set<FileProcessBatch>();
}
