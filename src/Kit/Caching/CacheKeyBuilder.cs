namespace NDB.Platform.Kit.Caching;

/// <summary>
/// Builder for consistent cache keys.
/// Format: "entity" or "entity:key1:key2".
/// </summary>
public static class CacheKeyBuilder
{
    /// <summary>
    /// Builds a cache key from an entity name and optional scope keys.
    /// </summary>
    /// <param name="entity">Entity name (e.g. "role", "page", "user").</param>
    /// <param name="keys">Optional scope keys (e.g. orgId, userId).</param>
    /// <returns>Cache key string.</returns>
    public static string For(string entity, params object?[] keys)
    {
        if (keys.Length == 0) return entity;
        var parts = new string[keys.Length + 1];
        parts[0] = entity;
        for (var i = 0; i < keys.Length; i++)
            parts[i + 1] = keys[i]?.ToString() ?? "null";
        return string.Join(':', parts);
    }
}
