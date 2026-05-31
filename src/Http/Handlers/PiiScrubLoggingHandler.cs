using Microsoft.Extensions.Logging;

namespace NDB.Platform.Http.Handlers;

/// <summary>
/// DelegatingHandler for logging outgoing HTTP requests and responses.
/// Logs only the method, URL, and status code — the body is never logged to prevent PII leakage.
/// </summary>
public sealed partial class PiiScrubLoggingHandler : DelegatingHandler
{
    private readonly ILogger<PiiScrubLoggingHandler> _logger;

    /// <summary>Initializes PiiScrubLoggingHandler.</summary>
    public PiiScrubLoggingHandler(ILogger<PiiScrubLoggingHandler> logger) =>
        _logger = logger;

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var method = request.Method.Method;
        var url = request.RequestUri?.AbsoluteUri ?? "(unknown)";

        // Log the request without the body to prevent PII leakage
        LogRequest(_logger, method, url);

        var response = await base.SendAsync(request, cancellationToken);

        // Log the response status code only
        LogResponse(_logger, method, url, (int)response.StatusCode);

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "HTTP {Method} {Url}")]
    private static partial void LogRequest(ILogger logger, string method, string url);

    [LoggerMessage(Level = LogLevel.Information, Message = "HTTP {Method} {Url} -> {StatusCode}")]
    private static partial void LogResponse(ILogger logger, string method, string url, int statusCode);
}
