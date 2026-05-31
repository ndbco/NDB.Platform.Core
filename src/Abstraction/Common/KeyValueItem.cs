namespace NDB.Platform.Abstraction.Common;

/// <summary>Generic key-value item for dropdowns, configuration, and metadata.</summary>
public sealed class KeyValueItem
{
    /// <summary>Key.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Value.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>Creates a new KeyValueItem.</summary>
    public KeyValueItem() { }

    /// <summary>Creates a new KeyValueItem with the specified key and value.</summary>
    public KeyValueItem(string key, string value)
    {
        Key = key;
        Value = value;
    }
}
