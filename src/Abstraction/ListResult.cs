#pragma warning disable CA1000  // Do not declare static members on generic types — by design for factory pattern
#pragma warning disable NDB001  // This file IS the factory definition — internal new() are allowed here
namespace NDB.Platform.Abstraction;

/// <summary>
/// Result for a non-paginated collection, including a total count.
/// </summary>
/// <typeparam name="T">The type of item in the collection.</typeparam>
public sealed class ListResult<T> : CollectionResult<T>
{
    /// <summary>Total number of items (may differ from Items.Count when a filter is applied).</summary>
    public int TotalCount { get; private init; }

    private ListResult() { }

    /// <summary>Creates a successful ListResult with the given items.</summary>
    public static ListResult<T> Success(IEnumerable<T> items, int? totalCount = null)
    {
        var list = items.ToList().AsReadOnly();
        return new ListResult<T>
        {
            Status = ResultStatus.Success,
            Items = list,
            TotalCount = totalCount ?? list.Count
        };
    }

    /// <summary>Creates a not-found ListResult.</summary>
    public static ListResult<T> NotFound(string? message = null) =>
        new() { Status = ResultStatus.NotFound, Message = message };

    /// <summary>Creates a bad-request ListResult.</summary>
    public static ListResult<T> BadRequest(string message) =>
        new() { Status = ResultStatus.BadRequest, Message = message };

    /// <summary>Creates an unauthorized ListResult.</summary>
    public static ListResult<T> Unauthorized(string? message = null) =>
        new() { Status = ResultStatus.Unauthorized, Message = message };

    /// <summary>Creates a forbidden ListResult.</summary>
    public static ListResult<T> Forbidden(string? message = null) =>
        new() { Status = ResultStatus.Forbidden, Message = message };

    /// <summary>Creates an internal-error ListResult.</summary>
    public static ListResult<T> Error(string message) =>
        new() { Status = ResultStatus.Error, Message = message };

    /// <summary>Creates a conflict ListResult.</summary>
    public static ListResult<T> Conflict(string? message = null) =>
        new() { Status = ResultStatus.Conflict, Message = message };
}
