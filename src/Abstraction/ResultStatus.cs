namespace NDB.Platform.Abstraction;

/// <summary>Status values for all Result responses.</summary>
public enum ResultStatus
{
    /// <summary>The operation succeeded.</summary>
    Success,

    /// <summary>The request was invalid.</summary>
    BadRequest,

    /// <summary>The requested resource was not found.</summary>
    NotFound,

    /// <summary>Access is unauthorized (login required).</summary>
    Unauthorized,

    /// <summary>Access is forbidden (insufficient permissions).</summary>
    Forbidden,

    /// <summary>Data conflict (duplicate, constraint violation).</summary>
    Conflict,

    /// <summary>An internal or unexpected error occurred.</summary>
    Error
}
