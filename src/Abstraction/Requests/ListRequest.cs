namespace NDB.Platform.Abstraction.Requests;

/// <summary>Combined request for list endpoints: paging + sort + filter.</summary>
public class ListRequest : PagingRequest
{
    /// <summary>Field name to sort by.</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort direction. Default: Asc.</summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Asc;

    /// <summary>Search keyword.</summary>
    public string? Keyword { get; set; }

    /// <summary>Fields covered by the keyword search. Null means all fields.</summary>
    public IReadOnlyList<string>? Fields { get; set; }
}
