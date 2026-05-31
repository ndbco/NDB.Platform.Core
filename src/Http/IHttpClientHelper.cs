namespace NDB.Platform.Http;

/// <summary>Contract for an HTTP client helper with typed results.</summary>
public interface IHttpClientHelper
{
    /// <summary>
    /// Sends an HTTP request and deserializes the response to type T.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="method">HTTP method.</param>
    /// <param name="url">Endpoint URL.</param>
    /// <param name="token">Optional Bearer token.</param>
    /// <param name="body">Optional request body (serialized to JSON).</param>
    /// <param name="options">Additional request options.</param>
    /// <param name="contentType">Content type of the request body.</param>
    /// <returns>HttpResult with data or error.</returns>
    Task<HttpResult<T>> SendAsync<T>(
        HttpMethod method,
        string url,
        string? token = null,
        object? body = null,
        RequestOptions? options = null,
        RequestContentType contentType = RequestContentType.Json);
}
