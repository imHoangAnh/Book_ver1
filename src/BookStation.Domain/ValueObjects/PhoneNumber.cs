using System.Text.RegularExpressions;
using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object representing a phone number.
/// </summary>
public sealed partial class PhoneNumber : ValueObject
{
    private const int MinLength = 9;
    private const int MaxLength = 20;

    /// <summary>
    /// Gets the phone number value.
    /// </summary>
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new PhoneNumber value object.
    /// </summary>
    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phone));

        // Remove spaces and dashes
        var cleaned = phone.Replace(" ", "").Replace("-", "").Replace(".", "");

        if (cleaned.Length < MinLength || cleaned.Length > MaxLength)
            throw new ArgumentException($"Phone number must be between {MinLength} and {MaxLength} digits.", nameof(phone));

        if (!PhoneRegex().IsMatch(cleaned))
            throw new ArgumentException("Phone number format is invalid.", nameof(phone));

        return new PhoneNumber(cleaned);
    }

    /// <summary>
    /// Tries to create a new PhoneNumber value object.
    /// </summary>
    public static bool TryCreate(string phone, out PhoneNumber? result)
    {
        try
        {
            result = Create(phone);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    [GeneratedRegex(@"^\+?[0-9]+$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();
}
