using System.Globalization;

namespace NDB.Platform.Kit.Format;

/// <summary>Number formatting for the Indonesian locale.</summary>
public static class NumberFormat
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    /// <summary>Formats a decimal as Indonesian Rupiah: "Rp 1.234.567,89".</summary>
    public static string FormatRupiah(decimal amount) =>
        $"Rp {amount.ToString("N2", IdCulture)}";

    /// <summary>Formats a decimal as a percentage: "12,50%".</summary>
    public static string FormatPercent(decimal value, int decimals = 2) =>
        $"{Math.Round(value, decimals).ToString("N" + decimals, IdCulture)}%";

    /// <summary>Formats a decimal with the Indonesian locale separator: "1.234,50".</summary>
    public static string FormatDecimal(decimal value, int decimals = 2) =>
        value.ToString("N" + decimals, IdCulture);

    /// <summary>Formats a long integer with thousands separators: "1.234.567".</summary>
    public static string FormatThousands(long value) =>
        value.ToString("N0", IdCulture);
}
