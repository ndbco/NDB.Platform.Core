namespace NDB.Platform.Abstraction.Storage;

/// <summary>
/// Provider-agnostic file storage abstraction (local, S3, Azure Blob, etc.).
/// The implementation is selected from configuration in the consuming project.
/// </summary>
public interface IFileStore
{
    /// <summary>Saves a stream to storage and returns the storage key.</summary>
    Task<string> SaveAsync(Stream content, string fileName, string mimeType, CancellationToken ct = default);

    /// <summary>Opens a stream from the given storage key.</summary>
    Task<Stream> OpenAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Deletes a file from storage.</summary>
    Task DeleteAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Checks whether a file exists in storage.</summary>
    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Name of the active provider (for metadata and logging).</summary>
    string ProviderName { get; }
}
