namespace BookStation.Core.Extensions;

/// <summary>
/// Extension methods for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether the string is null or whitespace.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Determines whether the string is not null and not whitespace.
    /// </summary>
    public static bool HasValue(this string? value) => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Trims the string and returns null if the result is empty.
    /// </summary>
    public static string? TrimToNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    /// <summary>
    /// Truncates the string to the specified maximum length.
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    /// <summary>
    /// Converts a string to snake_case.
    /// </summary>
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return string.Concat(
            value.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString())
        ).ToLowerInvariant();
    }
}
