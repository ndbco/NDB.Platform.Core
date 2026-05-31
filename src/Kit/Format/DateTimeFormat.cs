using System.Globalization;
namespace NDB.Platform.Kit.Format;

/// <summary>Date and time formatting for the Indonesian locale.</summary>
public static class DateTimeFormat
{
    private static readonly string[] BulanIndonesia =
    {
        "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember"
    };

    /// <summary>Formats a long Indonesian date: "20 Mei 2026".</summary>
    public static string FormatTanggalIndonesiaLong(DateTime dt) =>
        $"{dt.Day} {BulanIndonesia[dt.Month - 1]} {dt.Year}";

    /// <summary>Formats a short Indonesian date: "20/05/2026".</summary>
    public static string FormatTanggalIndonesiaShort(DateTime dt) =>
        $"{dt.Day:D2}/{dt.Month:D2}/{dt.Year}";

    /// <summary>Formats a time value: "14:30:00".</summary>
    public static string FormatJam(DateTime dt) =>
        dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats a relative time string in Indonesian.
    /// Examples: "baru saja", "3 menit lalu", "kemarin", "1 hari lalu".
    /// </summary>
    public static string FormatRelative(DateTime dt, DateTime? now = null)
    {
        var reference = now ?? DateTime.Now;
        var diff = reference - dt;

        if (diff.TotalSeconds < 60)
            return "baru saja";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} menit lalu";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} jam lalu";
        if (diff.TotalDays < 2)
            return "kemarin";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays} hari lalu";
        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)} minggu lalu";
        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)} bulan lalu";

        return $"{(int)(diff.TotalDays / 365)} tahun lalu";
    }
}
