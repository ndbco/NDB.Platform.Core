namespace NDB.Platform.Abstraction;

/// <summary>Pagination metadata for collection responses.</summary>
public sealed class PageInfo
{
    /// <summary>Current page number (1-based).</summary>
    public int Page { get; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalItems { get; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; }

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    private PageInfo(int page, int pageSize, int totalItems)
    {
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 0;
    }

    /// <summary>Creates a new PageInfo instance.</summary>
    public static PageInfo Create(int page, int pageSize, int totalItems) =>
        new(page, pageSize, totalItems);

    /// <summary>Empty PageInfo (for empty results).</summary>
    public static PageInfo Empty => new(0, 0, 0);
}
