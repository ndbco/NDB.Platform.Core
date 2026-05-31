using System.Globalization;

namespace NDB.Platform.Kit.Format;

/// <summary>Formats file sizes into human-readable strings.</summary>
public static class FileSizeFormat
{
    private static readonly CultureInfo IdCulture = new("id-ID");
    private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB" };

    /// <summary>Formats bytes into a human-readable string: "1,5 MB", "256 KB", etc.</summary>
    public static string FormatBytes(long bytes)
    {
        if (bytes < 0) return "0 B";
        if (bytes == 0) return "0 B";

        var index = 0;
        var value = (double)bytes;
        while (value >= 1024 && index < Units.Length - 1)
        {
            value /= 1024;
            index++;
        }

        return index == 0
            ? $"{(long)value} {Units[index]}"
            : $"{value.ToString("N1", IdCulture)} {Units[index]}";
    }

    /// <summary>Parses a human-readable size string to bytes. Example: "1.5 MB" → 1572864.</summary>
    public static long ParseToBytes(string humanReadable)
    {
        if (string.IsNullOrWhiteSpace(humanReadable)) return 0;

        var normalized = humanReadable.Trim().ToUpperInvariant()
            .Replace(",", "."); // normalize locale decimal separators

        for (var i = Units.Length - 1; i >= 0; i--)
        {
            var unit = Units[i];
            if (!normalized.EndsWith(unit, StringComparison.Ordinal)) continue;

            var numberPart = normalized[..^unit.Length].Trim();
            if (!double.TryParse(numberPart, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var number))
                return 0;

            return (long)(number * Math.Pow(1024, i));
        }

        return long.TryParse(normalized, out var raw) ? raw : 0;
    }
}
