using System.Text.Json.Serialization;

namespace ChatWYData.FileEntities;

public class FileUploadRequest
{

    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("FileContentType")]
    public string FileContentType { get; set; } = string.Empty;

    [JsonPropertyName("FileBytes")]
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
}
