namespace NDB.Platform.Kit.Guards;

/// <summary>Guard clauses for method argument validation.</summary>
public static class Guard
{
    /// <summary>Throws ArgumentNullException if the value is null.</summary>
    /// <typeparam name="T">Reference type.</typeparam>
    /// <param name="value">Value to check.</param>
    /// <param name="paramName">Parameter name (used in the error message).</param>
    /// <returns>The validated (non-null) value.</returns>
    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    /// <summary>Throws ArgumentException if the string is null or whitespace.</summary>
    /// <param name="value">Value to check.</param>
    /// <param name="paramName">Parameter name (used in the error message).</param>
    /// <returns>The validated (non-empty) value.</returns>
    public static string AgainstEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' must not be empty.", paramName);
        return value;
    }

    /// <summary>Throws ArgumentException if the value is the default value of its type.</summary>
    /// <typeparam name="T">Struct type.</typeparam>
    /// <param name="value">Value to check.</param>
    /// <param name="paramName">Parameter name (used in the error message).</param>
    /// <returns>The validated (non-default) value.</returns>
    public static T AgainstDefault<T>(T value, string paramName) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new ArgumentException($"'{paramName}' must not be the default value.", paramName);
        return value;
    }

    /// <summary>Throws ArgumentOutOfRangeException if the integer is negative.</summary>
    /// <param name="value">Value to check.</param>
    /// <param name="paramName">Parameter name (used in the error message).</param>
    /// <returns>The validated (>= 0) value.</returns>
    public static int AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must not be negative.");
        return value;
    }

    /// <summary>Throws ArgumentOutOfRangeException if the double is negative.</summary>
    /// <param name="value">Value to check.</param>
    /// <param name="paramName">Parameter name (used in the error message).</param>
    /// <returns>The validated (>= 0) value.</returns>
    public static double AgainstNegative(double value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must not be negative.");
        return value;
    }

    /// <summary>Throws ArgumentOutOfRangeException if the value is outside the min-max range.</summary>
    /// <typeparam name="T">Comparable type.</typeparam>
    /// <param name="value">Value to check.</param>
    /// <param name="min">Minimum bound (inclusive).</param>
    /// <param name="max">Maximum bound (inclusive).</param>
    /// <param name="paramName">Parameter name (used in the error message).</param>
    /// <returns>The validated value.</returns>
    public static T AgainstOutOfRange<T>(T value, T min, T max, string paramName)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(
                paramName, value, $"'{paramName}' must be between {min} and {max}.");
        return value;
    }

    /// <summary>Throws ArgumentException if the enum value is not defined.</summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="value">Enum value to check.</param>
    /// <param name="paramName">Parameter name (used in the error message).</param>
    /// <returns>The validated value.</returns>
    public static T AgainstInvalidEnum<T>(T value, string paramName) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException(
                $"'{paramName}' is not a valid enum value: {value}.", paramName);
        return value;
    }
}
