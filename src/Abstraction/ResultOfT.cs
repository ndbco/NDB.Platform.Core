#pragma warning disable CA1000 // Do not declare static members on generic types — by design for Result factory pattern
namespace NDB.Platform.Abstraction;

/// <summary>
/// Result with data — used for operations that return a payload.
/// Constructor is private; access only through static factory methods.
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public sealed class Result<T>
{
    /// <summary>Status of the operation result.</summary>
    public ResultStatus Status { get; }

    /// <summary>Data returned on success.</summary>
    public T? Data { get; }

    /// <summary>Optional message.</summary>
    public string? Message { get; }

    /// <summary>List of error messages (for multi-error BadRequest scenarios).</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Validation errors per field (for validation scenarios).</summary>
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    /// <summary>Exception that caused the error (optional, for logging/diagnostics).</summary>
    public Exception? Exception { get; }

    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess => Status == ResultStatus.Success;

    private Result(
        ResultStatus status,
        T? data,
        string? message,
        IReadOnlyList<string> errors,
        IReadOnlyDictionary<string, string[]>? validationErrors = null,
        Exception? exception = null)
    {
        Status = status;
        Data = data;
        Message = message;
        Errors = errors;
        ValidationErrors = validationErrors;
        Exception = exception;
    }

    // ── Static factories ───────────────────────────────────────────────────

    /// <summary>Creates a successful result with data.</summary>
    public static Result<T> Success(T data) =>
        new(ResultStatus.Success, data, null, Array.Empty<string>(), null);

    /// <summary>Creates a not-found result.</summary>
    public static Result<T> NotFound(string? message = null) =>
        new(ResultStatus.NotFound, default, message, Array.Empty<string>(), null);

    /// <summary>Creates a bad-request result with a list of errors.</summary>
    public static Result<T> BadRequest(IEnumerable<string> errors) =>
        new(ResultStatus.BadRequest, default, null, errors.ToList().AsReadOnly(), null);

    /// <summary>Creates a bad-request result with a single error message.</summary>
    public static Result<T> BadRequest(string message) =>
        new(ResultStatus.BadRequest, default, message, new[] { message }.AsReadOnly(), null);

    /// <summary>Creates an unauthorized result.</summary>
    public static Result<T> Unauthorized(string? message = null) =>
        new(ResultStatus.Unauthorized, default, message, Array.Empty<string>(), null);

    /// <summary>Creates a forbidden result.</summary>
    public static Result<T> Forbidden(string? message = null) =>
        new(ResultStatus.Forbidden, default, message, Array.Empty<string>(), null);

    /// <summary>Creates an internal-error result.</summary>
    public static Result<T> Error(string message) =>
        new(ResultStatus.Error, default, message, Array.Empty<string>(), null);

    /// <summary>
    /// Creates an internal-error result with an optional exception.
    /// Backward-compatible: <c>Result.Error&lt;T&gt;("msg")</c> still compiles via the parameterless overload.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="exception">Exception that caused the error. Null if not applicable.</param>
    public static Result<T> Error(string message, Exception? exception) =>
        new(ResultStatus.Error, default, message, Array.Empty<string>(), null, exception);

    /// <summary>Creates a conflict result.</summary>
    public static Result<T> Conflict(string? message = null) =>
        new(ResultStatus.Conflict, default, message, Array.Empty<string>(), null);

    /// <summary>Creates a per-field validation error result (for FluentValidation scenarios).</summary>
    public static Result<T> Validation(IDictionary<string, string[]> errors)
    {
        var flatErrors = errors.SelectMany(kv => kv.Value).ToList().AsReadOnly();
        IReadOnlyDictionary<string, string[]> readonlyErrors =
            new Dictionary<string, string[]>(errors).AsReadOnly();
        return new(ResultStatus.BadRequest, default, null, flatErrors, readonlyErrors);
    }
}
