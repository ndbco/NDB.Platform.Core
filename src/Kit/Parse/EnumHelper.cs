using System.ComponentModel;
using System.Reflection;

namespace NDB.Platform.Kit.Parse;

/// <summary>Enum helper utilities.</summary>
public static class EnumHelper
{
    /// <summary>
    /// Attempts to parse a string to enum T (case-insensitive).
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">The parsed result if successful.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParseEnum<T>(string value, out T result) where T : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out result);

    /// <summary>
    /// Returns all values of enum T.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>Read-only list of enum values.</returns>
    public static IReadOnlyList<T> ListValues<T>() where T : struct, Enum =>
        Enum.GetValues<T>();

    /// <summary>
    /// Retrieves the description from a DescriptionAttribute, or falls back to ToString() if none is present.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The enum description.</returns>
    public static string GetDescription<T>(T value) where T : struct, Enum
    {
        var memberInfo = typeof(T).GetMember(value.ToString());
        if (memberInfo.Length > 0)
        {
            var attr = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
            if (attr is not null)
                return attr.Description;
        }
        return value.ToString();
    }

    /// <summary>
    /// Finds an enum value by its DescriptionAttribute (case-insensitive).
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="description">The description to search for.</param>
    /// <returns>The matching enum value, or default if not found.</returns>
    public static T GetEnumFromDescription<T>(string description) where T : struct, Enum
    {
        foreach (var value in Enum.GetValues<T>())
        {
            var memberInfo = typeof(T).GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                var attr = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
                if (attr is not null &&
                    string.Equals(attr.Description, description, StringComparison.OrdinalIgnoreCase))
                    return value;
            }

            if (string.Equals(value.ToString(), description, StringComparison.OrdinalIgnoreCase))
                return value;
        }
        return default;
    }
}
