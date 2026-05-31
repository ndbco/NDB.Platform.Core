namespace NDB.Platform.Abstraction.Common;

/// <summary>Represents a stored file (upload result or file reference).</summary>
public sealed class FileObject
{
    /// <summary>Original file name.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>MIME type (e.g. image/jpeg, application/pdf).</summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long SizeBytes { get; init; }

    /// <summary>URL to access the file.</summary>
    public string Url { get; init; } = string.Empty;
}
