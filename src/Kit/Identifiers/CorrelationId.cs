namespace NDB.Platform.Kit.Identifiers;

/// <summary>Correlation ID scope helper using AsyncLocal for distributed tracing.</summary>
public static class CorrelationId
{
    /// <summary>HTTP header name for the correlation ID.</summary>
    public const string HeaderName = "X-Correlation-ID";

    private static readonly AsyncLocal<string?> Current = new();

    /// <summary>The current correlation ID in the async scope. Null if not set.</summary>
    public static string? Value
    {
        get => Current.Value;
        set => Current.Value = value;
    }

    /// <summary>
    /// Retrieves the current correlation ID, or creates a new one if none exists.
    /// </summary>
    /// <returns>The existing or newly created correlation ID.</returns>
    public static string GetOrCreate()
    {
        if (string.IsNullOrEmpty(Current.Value))
            Current.Value = Guid.NewGuid().ToString("N");
        return Current.Value;
    }
}
