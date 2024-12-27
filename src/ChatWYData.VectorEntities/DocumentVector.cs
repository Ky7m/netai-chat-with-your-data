using Microsoft.Extensions.VectorData;

namespace ChatWYData.VectorEntities;

public class DocumentVector 
{
    [VectorStoreRecordKey]
    public string? FileNameKey { get; set; }

    [VectorStoreRecordData]
    public string FileName { get; set; }

    [VectorStoreRecordData]
    public string FileDescription { get; set; }

    [VectorStoreRecordData]
    public string FileMarkDown { get; set; }

    [VectorStoreRecordData]
    public string FileContentType { get; set; }

    [VectorStoreRecordData]
    public string FileBlobUri { get; set; }

    [VectorStoreRecordData]
    public string CreatedAt { get; set; }

    [VectorStoreRecordData]
    public string UpdatedAt { get; set; }

    [VectorStoreRecordVector(1536, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
