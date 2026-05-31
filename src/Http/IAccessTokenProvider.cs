namespace NDB.Platform.Http;

/// <summary>
/// Abstraction for retrieving the current access token from context.
/// The default implementation is in NDB.Platform.API (HttpContext-based).
/// </summary>
public interface IAccessTokenProvider
{
    /// <summary>Retrieves the current access token (from HTTP context, cookie, or storage).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The token string, or null if not available.</returns>
    ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
