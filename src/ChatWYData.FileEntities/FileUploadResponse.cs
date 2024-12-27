namespace ChatWYData.FileEntities;

public class FileUploadResponse
{
    public string FileName { get; set; }
    public string MarkDown { get; set; }
    public string Description { get; set; }
    public string BlobUri { get; set; }
    public string VersionId { get; set; }

    // implement ToString() displaying all the properties values as return
    public override string ToString()
    {
        return $"FileName: {FileName}\nDescription: {Description}\nMarkDown: {MarkDown}\nBlobUri: {BlobUri}\nVersionId: {VersionId}";
    }

}