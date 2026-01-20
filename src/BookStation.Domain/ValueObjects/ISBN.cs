using System.Text.RegularExpressions;
using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object representing an ISBN (International Standard Book Number).
/// </summary>
public sealed partial class ISBN : ValueObject
{
    /// <summary>
    /// Gets the ISBN value.
    /// </summary>
    public string Value { get; }

    private ISBN(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new ISBN value object.
    /// </summary>
    public static ISBN Create(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN cannot be empty.", nameof(isbn));

        // Remove hyphens and spaces
        var cleaned = isbn.Replace("-", "").Replace(" ", "");

        // Validate ISBN-10 or ISBN-13
        if (!IsValidIsbn10(cleaned) && !IsValidIsbn13(cleaned))
            throw new ArgumentException("ISBN format is invalid.", nameof(isbn));

        return new ISBN(cleaned);
    }

    /// <summary>
    /// Tries to create a new ISBN value object.
    /// </summary>
    public static bool TryCreate(string isbn, out ISBN? result)
    {
        try
        {
            result = Create(isbn);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static bool IsValidIsbn10(string isbn)
    {
        if (isbn.Length != 10)
            return false;

        if (!Isbn10Regex().IsMatch(isbn))
            return false;

        // Calculate checksum
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (isbn[i] - '0') * (10 - i);
        }

        char lastChar = isbn[9];
        int lastDigit = lastChar == 'X' || lastChar == 'x' ? 10 : lastChar - '0';
        sum += lastDigit;

        return sum % 11 == 0;
    }

    private static bool IsValidIsbn13(string isbn)
    {
        if (isbn.Length != 13)
            return false;

        if (!Isbn13Regex().IsMatch(isbn))
            return false;

        // Calculate checksum
        int sum = 0;
        for (int i = 0; i < 13; i++)
        {
            int digit = isbn[i] - '0';
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// Gets the formatted ISBN with hyphens.
    /// </summary>
    public string Formatted
    {
        get
        {
            if (Value.Length == 13)
                return $"{Value[..3]}-{Value.Substring(3, 1)}-{Value.Substring(4, 4)}-{Value.Substring(8, 4)}-{Value[12]}";

            return $"{Value[..1]}-{Value.Substring(1, 4)}-{Value.Substring(5, 4)}-{Value[9]}";
        }
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;

    public static implicit operator string(ISBN isbn) => isbn.Value;

    [GeneratedRegex(@"^[0-9]{9}[0-9Xx]$", RegexOptions.Compiled)]
    private static partial Regex Isbn10Regex();

    [GeneratedRegex(@"^[0-9]{13}$", RegexOptions.Compiled)]
    private static partial Regex Isbn13Regex();
}
