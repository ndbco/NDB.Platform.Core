namespace NDB.Platform.Abstraction.Requests;

/// <summary>Basic pagination request.</summary>
public class PagingRequest
{
    /// <summary>Page number (1-based). Default: 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of items per page. Default: 20.</summary>
    public int PageSize { get; set; } = 20;
}
