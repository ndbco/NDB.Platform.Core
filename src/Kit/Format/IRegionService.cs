namespace NDB.Platform.Kit.Format;

/// <summary>
/// Abstraction for region-aware data formatting — currency, date, phone — according to locale.
/// The implementation is provided by the consuming project (using CultureInfo, regional data, etc.).
/// </summary>
/// <remarks>
/// Example outputs:
/// <list type="bullet">
/// <item>FormatCurrency(1_000_000m, "IDR", "id-ID") → "Rp 1.000.000"</item>
/// <item>FormatDate(dt, "id-ID") → "30 Mei 2026"</item>
/// <item>FormatPhone("0812345678") → "+6281234567" (E.164)</item>
/// </list>
/// </remarks>
public interface IRegionService
{
    /// <summary>
    /// Formats an amount according to the given currency code and locale.
    /// </summary>
    /// <param name="amount">Monetary amount.</param>
    /// <param name="currencyCode">ISO 4217 currency code (e.g. "IDR", "USD").</param>
    /// <param name="locale">BCP 47 locale tag (e.g. "id-ID"). Null uses the default locale.</param>
    string FormatCurrency(decimal amount, string currencyCode, string? locale = null);

    /// <summary>
    /// Formats a DateTime according to the given locale.
    /// </summary>
    /// <param name="dt">Date and time value.</param>
    /// <param name="locale">BCP 47 locale tag. Null uses the default locale.</param>
    string FormatDate(DateTime dt, string? locale = null);

    /// <summary>
    /// Normalizes a phone number to E.164 format.
    /// </summary>
    /// <param name="phone">Phone number in local or international format.</param>
    /// <returns>Number in E.164 format (e.g. "+6281234567").</returns>
    string FormatPhone(string phone);
}
