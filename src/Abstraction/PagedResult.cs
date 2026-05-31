#pragma warning disable CA1000  // Do not declare static members on generic types — by design for factory pattern
#pragma warning disable NDB001  // This file IS the factory definition — internal new() are allowed here
namespace NDB.Platform.Abstraction;

/// <summary>
/// Result for a fully paginated collection.
/// </summary>
/// <typeparam name="T">The type of item in the collection.</typeparam>
public sealed class PagedResult<T> : CollectionResult<T>
{
    /// <summary>Pagination metadata.</summary>
    public PageInfo PageInfo { get; private init; } = PageInfo.Empty;

    private PagedResult() { }

    /// <summary>Creates a successful PagedResult with items and pagination metadata.</summary>
    public static PagedResult<T> Success(IEnumerable<T> items, int page, int pageSize, int totalItems) =>
        new()
        {
            Status = ResultStatus.Success,
            Items = items.ToList().AsReadOnly(),
            PageInfo = PageInfo.Create(page, pageSize, totalItems)
        };

    /// <summary>Creates a not-found PagedResult.</summary>
    public static PagedResult<T> NotFound(string? message = null) =>
        new() { Status = ResultStatus.NotFound, Message = message };

    /// <summary>Creates a bad-request PagedResult.</summary>
    public static PagedResult<T> BadRequest(string message) =>
        new() { Status = ResultStatus.BadRequest, Message = message };

    /// <summary>Creates an unauthorized PagedResult.</summary>
    public static PagedResult<T> Unauthorized(string? message = null) =>
        new() { Status = ResultStatus.Unauthorized, Message = message };

    /// <summary>Creates a forbidden PagedResult.</summary>
    public static PagedResult<T> Forbidden(string? message = null) =>
        new() { Status = ResultStatus.Forbidden, Message = message };

    /// <summary>Creates an internal-error PagedResult.</summary>
    public static PagedResult<T> Error(string message) =>
        new() { Status = ResultStatus.Error, Message = message };

    /// <summary>Creates a conflict PagedResult.</summary>
    public static PagedResult<T> Conflict(string? message = null) =>
        new() { Status = ResultStatus.Conflict, Message = message };
}
