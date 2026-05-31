using System.Text.RegularExpressions;

namespace NDB.Platform.Kit.Text;

/// <summary>Compiled regex patterns for validating Indonesian data.</summary>
public static partial class RegexPatterns
{
    /// <summary>Standard email pattern.</summary>
    [GeneratedRegex(@"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled)]
    public static partial Regex Email();

    /// <summary>Indonesian phone number pattern: 08xx or +628xx, 10-13 total digits.</summary>
    [GeneratedRegex(@"^(\+62|62|0)8[1-9][0-9]{7,10}$", RegexOptions.Compiled)]
    public static partial Regex PhoneIndonesia();

    /// <summary>Indonesian NIK pattern: 16 numeric digits.</summary>
    [GeneratedRegex(@"^\d{16}$", RegexOptions.Compiled)]
    public static partial Regex Nik();

    /// <summary>Indonesian NPWP pattern: format XX.XXX.XXX.X-XXX.XXX.</summary>
    [GeneratedRegex(@"^\d{2}\.\d{3}\.\d{3}\.\d{1}-\d{3}\.\d{3}$", RegexOptions.Compiled)]
    public static partial Regex Npwp();

    /// <summary>Indonesian postal code pattern: 5 numeric digits.</summary>
    [GeneratedRegex(@"^\d{5}$", RegexOptions.Compiled)]
    public static partial Regex PostalCodeIndonesia();

    /// <summary>Alphanumeric-only pattern (A-Z, a-z, 0-9).</summary>
    [GeneratedRegex(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled)]
    public static partial Regex Alphanumeric();
}
