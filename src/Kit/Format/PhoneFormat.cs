using System.Text.RegularExpressions;

namespace NDB.Platform.Kit.Format;

/// <summary>Formatting and normalization of Indonesian phone numbers.</summary>
public static class PhoneFormat
{
    private static readonly Regex NonDigit = new(@"\D", RegexOptions.Compiled);

    /// <summary>
    /// Normalizes an Indonesian phone number to the international format "+62XXXXXXXXX".
    /// Supports formats: 08xxx, 628xxx, +628xxx.
    /// </summary>
    public static string NormalizeIndonesianPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = NonDigit.Replace(phone.Trim(), string.Empty);

        if (digits.StartsWith("62", StringComparison.Ordinal))
            return "+" + digits;
        if (digits.Length > 0 && digits[0] == '0')
            return "+62" + digits[1..];
        if (phone.TrimStart().Length > 0 && phone.TrimStart()[0] == '+')
            return "+" + digits;

        return "+62" + digits;
    }

    /// <summary>
    /// Formats a phone number for display: "0812-3456-7890".
    /// The input can be in any format; it is normalized first.
    /// </summary>
    public static string FormatDisplay(string phone)
    {
        var normalized = NormalizeIndonesianPhone(phone);
        var digits = NonDigit.Replace(normalized, string.Empty);

        // Convert +62 prefix to 0 for display
        if (digits.Length >= 2 && digits[..2] == "62")
            digits = "0" + digits[2..];

        // Format: XXXX-XXXX-XXXX
        return digits.Length switch
        {
            11 => $"{digits[..4]}-{digits[4..8]}-{digits[8..]}",
            12 => $"{digits[..4]}-{digits[4..8]}-{digits[8..]}",
            _ => digits
        };
    }
}
