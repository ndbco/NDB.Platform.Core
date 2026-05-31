using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NDB.Platform.Kit.Text;

/// <summary>String normalization utilities.</summary>
public static class StringNormalize
{
    private static readonly Regex SlugInvalidChars =
        new(@"[^a-z0-9-]", RegexOptions.Compiled);

    private static readonly Regex SlugMultipleDashes =
        new(@"-{2,}", RegexOptions.Compiled);

    private static readonly Regex PascalCasePattern =
        new(@"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled);

    private static readonly Regex MultipleSpaces =
        new(@"\s{2,}", RegexOptions.Compiled);

    /// <summary>Converts a string to a URL-friendly slug.</summary>
    public static string ToSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = RemoveDiacritics(value.ToLowerInvariant().Trim());
        var noSpaces = normalized.Replace(' ', '-');
        var cleaned = SlugInvalidChars.Replace(noSpaces, "-");
        var collapsed = SlugMultipleDashes.Replace(cleaned, "-");
        return collapsed.Trim('-');
    }

    /// <summary>Converts a PascalCase or camelCase string to kebab-case.</summary>
    public static string ToKebabCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return PascalCasePattern.Replace(value, "-").ToLowerInvariant();
    }

    /// <summary>Lowercases the first character (camelCase).</summary>
    public static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    /// <summary>Capitalizes the first letter of each word (Title Case / PascalCase).</summary>
    public static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return string.Join(string.Empty,
            value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));
    }

    /// <summary>Converts a PascalCase or camelCase string to snake_case.</summary>
    public static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return PascalCasePattern.Replace(value, "_").ToLowerInvariant();
    }

    /// <summary>Removes diacritic marks (accent characters) from a string.</summary>
    public static string RemoveDiacritics(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Trims a string and collapses multiple consecutive spaces into a single space.</summary>
    public static string TrimAndCollapseSpaces(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return MultipleSpaces.Replace(value.Trim(), " ");
    }
}
