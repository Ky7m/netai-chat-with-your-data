using System.ComponentModel.DataAnnotations;

namespace ChatWYData.DataEntities;

public class Document
{
    public Document()
    {
        FileName = string.Empty;
        FileDescription = "";
        FileMarkDown = "-";
        FileContentType = string.Empty;
        FileBlobUri = string.Empty;
        Chunks = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    [Key]
    public virtual string FileName { get; set; }
    public virtual string FileDescription { get; set; }
    public virtual string FileMarkDown { get; set; }
    public virtual string FileContentType { get; set; }
    public virtual string FileBlobUri { get; set; }
    public virtual int Chunks { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime UpdatedAt { get; set; }
}
