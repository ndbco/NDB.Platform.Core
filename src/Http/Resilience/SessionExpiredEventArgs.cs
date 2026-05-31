namespace NDB.Platform.Http.Resilience;

/// <summary>
/// Event args for the SessionExpired event in BaseApiService.
/// Raised when a token refresh fails (the token is expired and cannot be refreshed).
/// </summary>
public sealed class SessionExpiredEventArgs : EventArgs
{
    /// <summary>Reason the session expired (e.g. "Refresh token invalid", "Network error").</summary>
    public string? Reason { get; }

    /// <summary>Initializes SessionExpiredEventArgs.</summary>
    public SessionExpiredEventArgs(string? reason)
    {
        Reason = reason;
    }
}
