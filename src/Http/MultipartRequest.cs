namespace NDB.Platform.Http;

/// <summary>Model for a multipart file upload.</summary>
public sealed class MultipartRequest
{
    /// <summary>Field name in the multipart form. Default: "file".</summary>
    public string FieldName { get; init; } = "file";

    /// <summary>File name sent in the request. Default: "file".</summary>
    public string FileName { get; init; } = "file";

    /// <summary>File content stream.</summary>
    public Stream Content { get; init; } = Stream.Null;

    /// <summary>MIME type of the file. Default: "application/octet-stream".</summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>Additional fields included in the multipart form.</summary>
    public Dictionary<string, string> AdditionalFields { get; init; } = new();
}
