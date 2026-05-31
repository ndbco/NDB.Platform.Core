using NDB.Platform.Kit.Identifiers;

namespace NDB.Platform.Http.Handlers;

/// <summary>
/// DelegatingHandler that injects X-Correlation-ID into every outgoing HTTP request.
/// The ID is retrieved or created via CorrelationId.GetOrCreate().
/// </summary>
public sealed class CorrelationIdHandler : DelegatingHandler
{
    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var id = CorrelationId.GetOrCreate();
        request.Headers.TryAddWithoutValidation(CorrelationId.HeaderName, id);
        return base.SendAsync(request, cancellationToken);
    }
}
