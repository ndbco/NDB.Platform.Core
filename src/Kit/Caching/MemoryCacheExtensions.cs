using Microsoft.Extensions.Caching.Memory;

namespace NDB.Platform.Kit.Caching;

/// <summary>
/// Extension methods for IMemoryCache.
/// </summary>
public static class MemoryCacheExtensions
{
    /// <summary>
    /// Gets from cache; on a miss, invokes the factory, stores the result, and returns it.
    /// A null result from the factory is not cached (avoids caching negative lookups).
    /// </summary>
    /// <typeparam name="T">The type of data being cached.</typeparam>
    /// <param name="cache">IMemoryCache instance.</param>
    /// <param name="key">Cache key.</param>
    /// <param name="factory">Async factory to load data from the database on a cache miss.</param>
    /// <param name="options">Cache entry options (sliding + absolute expiry, priority, size).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Cached or fresh data, or null if the factory returns null.</returns>
    public static async Task<T?> GetOrSetAsync<T>(
        this IMemoryCache cache,
        string key,
        Func<Task<T?>> factory,
        MemoryCacheEntryOptions options,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(factory);

        if (cache.TryGetValue(key, out T? cached))
            return cached;

        ct.ThrowIfCancellationRequested();
        var value = await factory().ConfigureAwait(false);

        if (value is not null)
            cache.Set(key, value, options);

        return value;
    }

    /// <summary>
    /// Gets from cache; on a miss, invokes the factory and stores the result.
    /// Overload using the default master data options.
    /// </summary>
    public static Task<T?> GetOrSetAsync<T>(
        this IMemoryCache cache,
        string key,
        Func<Task<T?>> factory,
        CancellationToken ct = default)
        => cache.GetOrSetAsync(key, factory, CacheEntryDefaults.DefaultMasterDataOptions(), ct);
}
