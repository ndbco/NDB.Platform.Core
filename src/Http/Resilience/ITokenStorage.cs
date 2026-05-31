namespace NDB.Platform.Http.Resilience;

/// <summary>
/// Storage contract for access tokens and refresh tokens.
/// Default implementation: InMemoryTokenStorage (in NDB.Platform.Api).
/// Consuming projects can provide a custom implementation (cookie, secure storage, vault, etc.).
/// </summary>
public interface ITokenStorage
{
    /// <summary>Gets the current access token. Null if not logged in or already cleared.</summary>
    string? GetAccessToken();

    /// <summary>Gets the current refresh token. Null if not logged in or already cleared.</summary>
    string? GetRefreshToken();

    /// <summary>Persists the token pair after login or a successful refresh.</summary>
    void SetTokens(string accessToken, string refreshToken);

    /// <summary>Clears all tokens (logout).</summary>
    void ClearTokens();
}
