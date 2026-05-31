namespace NDB.Platform.Abstraction.Requests;

/// <summary>Sort direction.</summary>
public enum SortDirection
{
    /// <summary>Ascending (A-Z, 0-9).</summary>
    Asc,

    /// <summary>Descending (Z-A, 9-0).</summary>
    Desc
}

/// <summary>Sort request.</summary>
public class SortRequest
{
    /// <summary>Field name to sort by.</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort direction. Default: Asc.</summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Asc;
}
