using System.Text;

namespace NDB.Platform.Kit.Crypto;

/// <summary>Blake3 one-way hash for PII data (national ID, email, phone number, etc.).</summary>
public static class PiiHasher
{
    /// <summary>
    /// Hashes a PII value using Blake3.
    /// Deterministic: the same input always produces the same hash.
    /// </summary>
    /// <param name="value">The PII value to hash.</param>
    /// <param name="salt">Optional salt for isolation between tenants or domains.</param>
    /// <returns>Lowercase hex string of the Blake3 hash.</returns>
    public static string Hash(string value, string? salt = null)
    {
        var input = salt is null ? value : $"{salt}:{value}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = Blake3.Hasher.Hash(bytes);
        return Convert.ToHexString(hash.AsSpan()).ToLowerInvariant();
    }
}
