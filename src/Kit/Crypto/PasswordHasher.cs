using BC = BCrypt.Net.BCrypt;

namespace NDB.Platform.Kit.Crypto;

/// <summary>BCrypt password hashing utilities.</summary>
public static class PasswordHasher
{
    /// <summary>Hashes a password using BCrypt with a work factor of 12.</summary>
    /// <param name="plainText">Plain-text password.</param>
    /// <returns>BCrypt hash.</returns>
    public static string Hash(string plainText) =>
        BC.HashPassword(plainText, workFactor: 12);

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    /// <param name="plainText">Plain-text password.</param>
    /// <param name="hashedPassword">Stored BCrypt hash.</param>
    /// <returns>True if the password matches.</returns>
    public static bool Verify(string plainText, string hashedPassword) =>
        BC.Verify(plainText, hashedPassword);
}
