using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NDB.Platform.Kit.Caching;

/// <summary>
/// Extension methods for <see cref="IDistributedCache"/> — cache-aside pattern.
/// Complements <see cref="MemoryCacheExtensions"/> for Redis / distributed cache scenarios.
/// </summary>
public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets from cache; on a miss, invokes the factory, stores the result, and returns it.
    /// A null result from the factory is not cached (avoids caching negative lookups).
    /// </summary>
    /// <typeparam name="T">The type of data being cached.</typeparam>
    /// <param name="cache">IDistributedCache instance.</param>
    /// <param name="key">Cache key.</param>
    /// <param name="factory">Async factory to load data from the source on a cache miss.</param>
    /// <param name="options">Cache entry options (AbsoluteExpiration / SlidingExpiration).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Cached or fresh data, or null if the factory returns null.</returns>
    public static async Task<T?> GetOrSetAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T?>> factory,
        DistributedCacheEntryOptions options,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(factory);

        var bytes = await cache.GetAsync(key, ct).ConfigureAwait(false);
        if (bytes is not null)
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);

        ct.ThrowIfCancellationRequested();
        var value = await factory().ConfigureAwait(false);

        if (value is not null)
        {
            var serialized = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await cache.SetAsync(key, serialized, options, ct).ConfigureAwait(false);
        }

        return value;
    }

    /// <summary>
    /// Standard master-data TTL: absolute 15 minutes.
    /// Suitable for: roles, permissions, pages, settings, module lists.
    /// </summary>
    public static DistributedCacheEntryOptions MasterDataOptions() =>
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };

    /// <summary>
    /// Standard reference-data TTL: absolute 30 minutes.
    /// Suitable for: locale lists, branding, currencies, number sequences.
    /// </summary>
    public static DistributedCacheEntryOptions ReferenceDataOptions() =>
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };

    /// <summary>
    /// Short-lived cache TTL: absolute 5 minutes.
    /// Suitable for: entity-specific cache (tags per entity, user permissions).
    /// </summary>
    public static DistributedCacheEntryOptions ShortLivedOptions() =>
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
}
