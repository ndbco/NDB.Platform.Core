namespace NDB.Platform.Abstraction.Requests;

/// <summary>Filter/search request.</summary>
public class FilterRequest
{
    /// <summary>Search keyword.</summary>
    public string? Keyword { get; set; }

    /// <summary>Fields covered by the keyword search. Null means all fields.</summary>
    public IReadOnlyList<string>? Fields { get; set; }
}
