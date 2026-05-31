namespace NDB.Platform.Abstraction;

/// <summary>
/// Identity of the actor performing an operation — used for audit trail, logging, and cross-cutting concerns.
/// Immutable record; use <c>with</c> expressions for non-destructive updates.
/// </summary>
public sealed record AuditActor
{
    /// <summary>Display name or username of the actor (e.g. "admin", "john.doe@email.com").</summary>
    public string? Actor { get; init; }

    /// <summary>User ID of the actor as a string (e.g. Guid string or integer string).</summary>
    public string? ActorId { get; init; }

    /// <summary>Primary role of the actor (e.g. "ADMIN", "SUPERVISOR").</summary>
    public string? Role { get; init; }

    /// <summary>Correlation ID for distributed tracing.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Sentinel actor for background jobs, CLI, and anonymous requests.
    /// Always returns an actor with the name "SYSTEM" and no ActorId or Role.
    /// </summary>
    public static AuditActor System { get; } = new()
    {
        Actor = "SYSTEM",
        ActorId = null,
        Role = null,
        CorrelationId = null
    };
}
