#pragma warning disable CA1000 // Do not declare static members on generic types — by design for factory pattern
namespace NDB.Platform.Http;

/// <summary>HTTP call result without data.</summary>
public sealed class HttpResult
{
    /// <summary>Whether the request succeeded (HTTP 2xx).</summary>
    public bool Succeeded { get; }

    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>Error message if the request failed.</summary>
    public string? ErrorMessage { get; }

    /// <summary>Raw response body as a string.</summary>
    public string? Raw { get; }

    private HttpResult(bool succeeded, int statusCode, string? errorMessage, string? raw)
    {
        Succeeded = succeeded;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        Raw = raw;
    }

    /// <summary>Creates a successful HttpResult.</summary>
    public static HttpResult Ok(int statusCode, string? raw = null) =>
        new(true, statusCode, null, raw);

    /// <summary>Creates a failed HttpResult.</summary>
    public static HttpResult Fail(int statusCode, string? errorMessage, string? raw = null) =>
        new(false, statusCode, errorMessage, raw);
}

/// <summary>HTTP call result with data.</summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public sealed class HttpResult<T>
{
    /// <summary>Whether the request succeeded (HTTP 2xx).</summary>
    public bool Succeeded { get; }

    /// <summary>Data deserialized from the response body.</summary>
    public T? Data { get; }

    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; }

    /// <summary>Error message if the request failed.</summary>
    public string? ErrorMessage { get; }

    /// <summary>Raw response body as a string.</summary>
    public string? Raw { get; }

    /// <summary>
    /// Response headers from the HTTP response.
    /// Used to detect custom headers such as <c>Token-Expired: true</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Headers { get; }

    private HttpResult(
        bool succeeded,
        T? data,
        int statusCode,
        string? errorMessage,
        string? raw,
        IReadOnlyDictionary<string, string[]>? headers = null)
    {
        Succeeded = succeeded;
        Data = data;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        Raw = raw;
        Headers = headers;
    }

    /// <summary>Creates a successful HttpResult with data.</summary>
    public static HttpResult<T> Ok(
        T data,
        int statusCode,
        string? raw = null,
        IReadOnlyDictionary<string, string[]>? headers = null) =>
        new(true, data, statusCode, null, raw, headers);

    /// <summary>Creates a failed HttpResult.</summary>
    public static HttpResult<T> Fail(
        int statusCode,
        string? errorMessage,
        string? raw = null,
        IReadOnlyDictionary<string, string[]>? headers = null) =>
        new(false, default, statusCode, errorMessage, raw, headers);
}
