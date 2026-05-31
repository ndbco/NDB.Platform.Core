namespace NDB.Platform.Abstraction;

/// <summary>Base result for a collection of items.</summary>
/// <typeparam name="T">The type of item in the collection.</typeparam>
public abstract class CollectionResult<T>
{
    /// <summary>Status of the operation result.</summary>
    public ResultStatus Status { get; protected init; }

    /// <summary>Optional message.</summary>
    public string? Message { get; protected init; }

    /// <summary>List of items returned by the operation.</summary>
    public IReadOnlyList<T> Items { get; protected init; } = Array.Empty<T>();

    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess => Status == ResultStatus.Success;

    /// <summary>Protected constructor — accessible only by subclasses.</summary>
    protected CollectionResult() { }
}
