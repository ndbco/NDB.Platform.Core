namespace NDB.Platform.Abstraction.Common;

/// <summary>Reference item with a Guid key.</summary>
public sealed class ReferenceItem
{
    /// <summary>Item identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Display label of the item.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Creates a new ReferenceItem.</summary>
    public ReferenceItem() { }

    /// <summary>Creates a new ReferenceItem with the specified id and name.</summary>
    public ReferenceItem(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
