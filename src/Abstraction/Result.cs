namespace NDB.Platform.Abstraction;

/// <summary>
/// Result without data — used for operations that return no payload (e.g. delete, void update).
/// Constructor is private; access only through static factory methods.
/// </summary>
public sealed class Result
{
    /// <summary>Status of the operation result.</summary>
    public ResultStatus Status { get; }

    /// <summary>Optional message for success or single-error scenarios.</summary>
    public string? Message { get; }

    /// <summary>List of error messages (for multi-error BadRequest scenarios).</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Exception that caused the error (optional, for logging/diagnostics).</summary>
    public Exception? Exception { get; }

    /// <summary>Validation errors per field (for validation scenarios).</summary>
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess => Status == ResultStatus.Success;

    private Result(
        ResultStatus status,
        string? message,
        IReadOnlyList<string> errors,
        Exception? exception = null,
        IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        Status = status;
        Message = message;
        Errors = errors;
        Exception = exception;
        ValidationErrors = validationErrors;
    }

    // ── Non-generic factories ──────────────────────────────────────────────

    /// <summary>Creates a successful result with no data.</summary>
    public static Result Success(string? message = null) =>
        new(ResultStatus.Success, message, Array.Empty<string>());

    /// <summary>Creates a not-found result.</summary>
    public static Result NotFound(string? message = null) =>
        new(ResultStatus.NotFound, message, Array.Empty<string>());

    /// <summary>Creates a bad-request result with a list of errors.</summary>
    public static Result BadRequest(IEnumerable<string> errors) =>
        new(ResultStatus.BadRequest, null, errors.ToList().AsReadOnly());

    /// <summary>Creates a bad-request result with a single error message.</summary>
    public static Result BadRequest(string message) =>
        new(ResultStatus.BadRequest, message, new[] { message }.AsReadOnly());

    /// <summary>Creates an unauthorized result.</summary>
    public static Result Unauthorized(string? message = null) =>
        new(ResultStatus.Unauthorized, message, Array.Empty<string>());

    /// <summary>Creates a forbidden result.</summary>
    public static Result Forbidden(string? message = null) =>
        new(ResultStatus.Forbidden, message, Array.Empty<string>());

    /// <summary>Creates an internal-error result.</summary>
    public static Result Error(string message) =>
        new(ResultStatus.Error, message, Array.Empty<string>());

    /// <summary>
    /// Creates an internal-error result with an optional exception.
    /// Backward-compatible: <c>Result.Error("msg")</c> still compiles via the parameterless overload.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="exception">Exception that caused the error. Null if not applicable.</param>
    public static Result Error(string message, Exception? exception) =>
        new(ResultStatus.Error, message, Array.Empty<string>(), exception);

    /// <summary>
    /// Creates a per-field validation error result.
    /// For FluentValidation / model validation scenarios (non-generic).
    /// </summary>
    /// <param name="errors">Dictionary of field → string[] error messages.</param>
    /// <param name="message">Optional message. Default: "Validation failed".</param>
    public static Result Validation(IDictionary<string, string[]> errors, string? message = null)
    {
        var flatErrors = errors.SelectMany(kv => kv.Value).ToList().AsReadOnly();
        IReadOnlyDictionary<string, string[]> readonlyErrors =
            new Dictionary<string, string[]>(errors).AsReadOnly();
        return new(ResultStatus.BadRequest, message ?? "Validation failed", flatErrors,
            validationErrors: readonlyErrors);
    }

    /// <summary>Creates a conflict result.</summary>
    public static Result Conflict(string? message = null) =>
        new(ResultStatus.Conflict, message, Array.Empty<string>());

    // ── Generic delegation ─────────────────────────────────────────────────

    /// <summary>Creates a successful result with data.</summary>
    public static Result<T> Success<T>(T data) => Result<T>.Success(data);

    /// <summary>Creates a generic not-found result.</summary>
    public static Result<T> NotFound<T>(string? message = null) => Result<T>.NotFound(message);

    /// <summary>Creates a generic bad-request result with a list of errors.</summary>
    public static Result<T> BadRequest<T>(IEnumerable<string> errors) => Result<T>.BadRequest(errors);

    /// <summary>Creates a generic error result.</summary>
    public static Result<T> Error<T>(string message) => Result<T>.Error(message);

    /// <summary>
    /// Creates a generic error result with an optional exception.
    /// </summary>
    public static Result<T> Error<T>(string message, Exception? exception) => Result<T>.Error(message, exception);

    /// <summary>Creates a generic conflict result.</summary>
    public static Result<T> Conflict<T>(string? message = null) => Result<T>.Conflict(message);
}
