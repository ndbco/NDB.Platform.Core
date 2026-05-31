namespace NDB.Platform.Http.Resilience;

/// <summary>
/// Response from a token refresh operation.
/// </summary>
public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
