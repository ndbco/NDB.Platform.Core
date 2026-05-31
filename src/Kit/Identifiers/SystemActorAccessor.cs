using NDB.Platform.Abstraction;

namespace NDB.Platform.Kit.Identifiers;

/// <summary>
/// Default <see cref="IActorAccessor"/> for background jobs, CLI tools, and worker services without an HttpContext.
/// Always returns <see cref="AuditActor.System"/> with the correlation ID from the current AsyncLocal scope.
/// </summary>
/// <remarks>
/// Registered as <c>TryAddSingleton</c> by <c>AddNdbCqrs()</c> — can be overridden
/// by <c>AddNdbJwt()</c> (which registers <c>HttpContextActorAccessor</c> as Scoped).
/// </remarks>
public sealed class SystemActorAccessor : IActorAccessor
{
    /// <inheritdoc />
    public AuditActor GetCurrent() => AuditActor.System with
    {
        CorrelationId = CorrelationId.Value
    };
}
