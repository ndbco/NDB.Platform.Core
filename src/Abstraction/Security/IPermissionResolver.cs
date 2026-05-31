namespace NDB.Platform.Abstraction.Security;

/// <summary>
/// Contract for resolving a user's effective permissions — role-based RBAC with DENY override.
/// The implementation is provided by the consuming project (with DB/cache access).
/// </summary>
/// <remarks>
/// Algorithm:
/// effective_permissions = union(role_permissions)
///     + union(grants ALLOW)
///     - union(grants DENY)
/// DENY always takes precedence over ALLOW.
/// Superadmin has all permissions (bypassed in NDB.Platform.Api.Authorization.PermissionAuthorizationHandler).
/// </remarks>
public interface IPermissionResolver
{
    /// <summary>Returns all effective permission keys for the given user.</summary>
    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Checks whether the user has a specific permission key.</summary>
    Task<bool> HasPermissionAsync(Guid userId, string permissionKey, CancellationToken ct = default);

    /// <summary>Invalidates the permission cache for the user (call when roles, grants, or overrides change).</summary>
    Task InvalidateAsync(Guid userId, CancellationToken ct = default);
}
