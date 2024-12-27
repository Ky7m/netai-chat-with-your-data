using System.ComponentModel.DataAnnotations;

namespace ChatWYData.DataEntities;

public class FileProcessBatch
{
    [Key]
    public string FileName { get; set; } = string.Empty;
    public string FileBlobUri { get; set; } = string.Empty;
    public string FileContentType { get; set; } = string.Empty;
    public string StatusMessage { get; set; }   = string.Empty;
    public string StepsDescription { get; set; } = string.Empty;
    public bool Processed { get; set; } = false;
    public bool Ignore { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    public DateTime CompletedDate { get; set; } = DateTime.MaxValue;
}
