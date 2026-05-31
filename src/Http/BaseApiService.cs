using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NDB.Platform.Http.Resilience;

namespace NDB.Platform.Http;

/// <summary>
/// Base class for typed HTTP client services.
/// Subclasses inject an HttpClient and use the verb shortcut methods.
/// Supports automatic JWT refresh when a 401 with a Token-Expired header is detected.
/// </summary>
public abstract partial class BaseApiService : IDisposable
{
    /// <summary>HttpClient used for requests.</summary>
    protected HttpClient HttpClient { get; }

    /// <summary>Logger for diagnostics.</summary>
    protected ILogger Logger { get; }

    private readonly ITokenRefresher? _refresher;
    private readonly ITokenStorage? _tokenStorage;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Event raised when a token refresh fails (session is truly expired).
    /// Subscribers can handle logout or redirect to the login page.
    /// </summary>
    public event EventHandler<SessionExpiredEventArgs>? SessionExpired;

    /// <summary>Initializes BaseApiService without JWT refresh support.</summary>
    protected BaseApiService(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
    }

    /// <summary>Initializes BaseApiService with JWT auto-refresh support.</summary>
    protected BaseApiService(
        HttpClient httpClient,
        ILogger logger,
        ITokenRefresher refresher,
        ITokenStorage tokenStorage)
        : this(httpClient, logger)
    {
        _refresher = refresher;
        _tokenStorage = tokenStorage;
    }

    /// <summary>HTTP GET and deserialize the response to T.</summary>
    protected Task<HttpResult<T>> GetAsync<T>(string path, CancellationToken ct = default) =>
        SendWithRefreshAsync<T>(HttpMethod.Get, path, null, ct);

    /// <summary>HTTP POST with a JSON body and deserialize the response to T.</summary>
    protected Task<HttpResult<T>> PostAsync<T>(string path, object body, CancellationToken ct = default) =>
        SendWithRefreshAsync<T>(HttpMethod.Post, path, body, ct);

    /// <summary>HTTP PUT with a JSON body and deserialize the response to T.</summary>
    protected Task<HttpResult<T>> PutAsync<T>(string path, object body, CancellationToken ct = default) =>
        SendWithRefreshAsync<T>(HttpMethod.Put, path, body, ct);

    /// <summary>HTTP DELETE and deserialize the response to T.</summary>
    protected Task<HttpResult<T>> DeleteAsync<T>(string path, CancellationToken ct = default) =>
        SendWithRefreshAsync<T>(HttpMethod.Delete, path, null, ct);

    /// <summary>HTTP POST with a form-urlencoded body.</summary>
    protected async Task<HttpResult<T>> PostFormAsync<T>(
        string path,
        Dictionary<string, string> formData,
        CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(formData);
        var resp = await HttpClient.PostAsync(path, content, ct);
        return await ParseResponseAsync<T>(resp);
    }

    /// <summary>HTTP POST with a multipart/form-data body (file upload).</summary>
    protected async Task<HttpResult<T>> PostMultipartAsync<T>(
        string path,
        MultipartRequest req,
        CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(req.Content);
        streamContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(req.ContentType);
        content.Add(streamContent, req.FieldName, req.FileName);
        foreach (var field in req.AdditionalFields)
            content.Add(new StringContent(field.Value), field.Key);
        var resp = await HttpClient.PostAsync(path, content, ct);
        return await ParseResponseAsync<T>(resp);
    }

    private async Task<HttpResult<T>> SendWithRefreshAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken ct)
    {
        var result = await SendCoreAsync<T>(method, path, body, ct);

        // JWT auto-refresh: only refresh when 401 AND the Token-Expired: true header is present.
        // A 401 without this header (e.g. wrong password, bad request) should not trigger a refresh.
        if (result.StatusCode == (int)HttpStatusCode.Unauthorized
            && _refresher is not null
            && _tokenStorage is not null
            && IsTokenExpiredHeader(result.Headers))
        {
            var refreshed = await TryRefreshAsync(ct);
            if (refreshed)
                return await SendCoreAsync<T>(method, path, body, ct);
        }

        return result;
    }

    /// <summary>
    /// Checks whether the response headers contain <c>Token-Expired: true</c>.
    /// Returns true only when the header is present AND its value is "true" (case-insensitive).
    /// </summary>
    private static bool IsTokenExpiredHeader(IReadOnlyDictionary<string, string[]>? headers)
    {
        if (headers is null) return false;
        if (!headers.TryGetValue("Token-Expired", out var values)) return false;
        return values.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        // Capture the current token before acquiring the lock (for double-check on race conditions)
        var entryToken = _tokenStorage!.GetAccessToken();

        // Double-check pattern with SemaphoreSlim to prevent refresh storms
        await _refreshLock.WaitAsync(ct);
        try
        {
            // Double-check: if the token has already changed while waiting for the lock,
            // another concurrent request has successfully refreshed — exit early.
            var currentToken = _tokenStorage.GetAccessToken();
            if (!string.IsNullOrEmpty(currentToken) && currentToken != entryToken)
            {
                // Token was already refreshed by a concurrent waiter — use it directly
                HttpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentToken);
                return true;
            }

            var refreshResult = await _refresher!.RefreshAsync(ct);
            if (!refreshResult.IsSuccess)
            {
                OnSessionExpired(refreshResult.Message ?? "Refresh token invalid or expired");
                return false;
            }

            // Update the Authorization header with the new token
            HttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", refreshResult.Data!.AccessToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            LogRefreshFailed(Logger, ex);
            OnSessionExpired($"Refresh failed: {ex.Message}");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <summary>Raises the SessionExpired event.</summary>
    protected virtual void OnSessionExpired(string? reason) =>
        SessionExpired?.Invoke(this, new SessionExpiredEventArgs(reason));

    private async Task<HttpResult<T>> SendCoreAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken ct)
    {
        HttpContent? content = body is not null
            ? JsonContent.Create(body, options: JsonOptions)
            : null;
        var request = new HttpRequestMessage(method, path) { Content = content };
        var resp = await HttpClient.SendAsync(request, ct);
        return await ParseResponseAsync<T>(resp);
    }

    private async Task<HttpResult<T>> ParseResponseAsync<T>(HttpResponseMessage resp)
    {
        var raw = await resp.Content.ReadAsStringAsync();

        // Capture response headers (including custom headers such as Token-Expired)
        var headers = (IReadOnlyDictionary<string, string[]>)resp.Headers
            .ToDictionary(h => h.Key, h => h.Value.ToArray(), StringComparer.OrdinalIgnoreCase);

        if (!resp.IsSuccessStatusCode)
            return HttpResult<T>.Fail((int)resp.StatusCode, resp.ReasonPhrase, raw, headers);

        try
        {
            var data = JsonSerializer.Deserialize<T>(raw, JsonOptions);
            return HttpResult<T>.Ok(data!, (int)resp.StatusCode, raw, headers);
        }
        catch (JsonException ex)
        {
            LogDeserializeError(Logger, ex,
                resp.RequestMessage?.Method.ToString() ?? "?",
                resp.RequestMessage?.RequestUri?.ToString() ?? "?");
            return HttpResult<T>.Fail(
                (int)resp.StatusCode, $"Deserialization error: {ex.Message}", raw, headers);
        }
    }

    /// <summary>Disposes resources owned by BaseApiService.</summary>
    public virtual void Dispose()
    {
        _refreshLock.Dispose();
        GC.SuppressFinalize(this);
    }

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Failed to deserialize response from {Method} {Url}")]
    private static partial void LogDeserializeError(ILogger logger, Exception ex, string method, string url);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Token refresh failed unexpectedly")]
    private static partial void LogRefreshFailed(ILogger logger, Exception ex);
}
