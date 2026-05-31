using System.Text;

namespace NDB.Platform.Kit.Crypto;

/// <summary>Base64 encode/decode utilities.</summary>
public static class Base64Helper
{
    /// <summary>Encodes a string to standard Base64.</summary>
    public static string Encode(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    /// <summary>Decodes a standard Base64 string.</summary>
    public static string Decode(string base64) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(base64));

    /// <summary>Encodes a string to URL-safe Base64 (no padding, uses - and _).</summary>
    public static string EncodeUrlSafe(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    /// <summary>Decodes a URL-safe Base64 string.</summary>
    public static string DecodeUrlSafe(string base64Url)
    {
        var padded = base64Url.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
