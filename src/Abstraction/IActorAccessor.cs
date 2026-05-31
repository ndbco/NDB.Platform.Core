namespace NDB.Platform.Abstraction;

/// <summary>
/// Resolves the identity of the current actor for audit trail, logging, and cross-cutting concerns.
/// </summary>
/// <remarks>
/// Available implementations:
/// <list type="bullet">
/// <item><term>Web/API</term><description><c>HttpContextActorAccessor</c> from NDB.Platform.Api — reads claims from the JWT in HttpContext.</description></item>
/// <item><term>Background job / CLI</term><description><c>SystemActorAccessor</c> from NDB.Platform.Core — always returns <see cref="AuditActor.System"/>.</description></item>
/// <item><term>Custom</term><description>Consumer implementation (e.g. machine-to-machine with a service account).</description></item>
/// </list>
/// <para>
/// Registration: <c>AddNdbCqrs()</c> registers <c>SystemActorAccessor</c> as a fallback (<c>TryAddSingleton</c>).
/// <c>AddNdbJwt()</c> overrides it with <c>HttpContextActorAccessor</c> (Scoped) for web projects.
/// </para>
/// </remarks>
public interface IActorAccessor
{
    /// <summary>
    /// Returns the current actor.
    /// Never returns null — returns <see cref="AuditActor.System"/> when no context is available.
    /// </summary>
    AuditActor GetCurrent();
}
