using Microsoft.Extensions.Caching.Memory;

namespace NDB.Platform.Kit.Caching;

/// <summary>
/// Default MemoryCacheEntryOptions presets for common use cases.
/// </summary>
public static class CacheEntryDefaults
{
    /// <summary>
    /// Options for master data (roles, permissions, pages, organizations).
    /// Sliding: 60s, Absolute: 1 hour, Priority: Normal, Size: 1024.
    /// </summary>
    public static MemoryCacheEntryOptions DefaultMasterDataOptions() =>
        new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(60))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(1024);

    /// <summary>
    /// Options for reference data that rarely changes (settings, config).
    /// Sliding: 5 minutes, Absolute: 24 hours.
    /// </summary>
    public static MemoryCacheEntryOptions DefaultReferenceOptions() =>
        new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromHours(24));

    /// <summary>
    /// Options for more dynamic lookup data (recent activities, notifications).
    /// Sliding: 30s, Absolute: 5 minutes.
    /// </summary>
    public static MemoryCacheEntryOptions DefaultLookupOptions() =>
        new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(30))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
}
