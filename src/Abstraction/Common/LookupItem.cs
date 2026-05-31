namespace NDB.Platform.Abstraction.Common;

/// <summary>Generic lookup item with a string key.</summary>
public sealed class LookupItem
{
    /// <summary>Item identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Display label of the item.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Creates a new LookupItem.</summary>
    public LookupItem() { }

    /// <summary>Creates a new LookupItem with the specified id and name.</summary>
    public LookupItem(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
