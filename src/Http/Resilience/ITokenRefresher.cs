using NDB.Platform.Abstraction;

namespace NDB.Platform.Http.Resilience;

/// <summary>
/// Contract for the token refresh operation.
/// Default implementation: DefaultTokenRefresher (in NDB.Platform.Api).
/// </summary>
public interface ITokenRefresher
{
    /// <summary>
    /// Refreshes the access token using the refresh token stored in ITokenStorage.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the new TokenResponse, or an error if the refresh failed.</returns>
    Task<Result<TokenResponse>> RefreshAsync(CancellationToken ct = default);
}
