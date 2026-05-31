using System.ComponentModel;

namespace NDB.Platform.Kit.Parse;

/// <summary>Generic parse utilities using TypeConverter.</summary>
public static class Parse
{
    /// <summary>
    /// Attempts to parse a string to type T.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">The parsed result if successful.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParse<T>(string value, out T result)
    {
        result = default!;
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
            {
                var converted = converter.ConvertFromInvariantString(value);
                if (converted is T typed)
                {
                    result = typed;
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parses a string to type T. Throws FormatException on failure.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed result.</returns>
    public static T ParseValue<T>(string value)
    {
        if (TryParse<T>(value, out var result))
            return result;
        throw new FormatException(
            $"Cannot parse '{value}' to type {typeof(T).Name}.");
    }
}
