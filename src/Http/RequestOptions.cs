namespace NDB.Platform.Http;

/// <summary>Additional options for an HTTP request.</summary>
public sealed class RequestOptions
{
    /// <summary>Additional headers to include in the request.</summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>Custom timeout for this request. Null uses the HttpClient default.</summary>
    public TimeSpan? Timeout { get; init; }
}
