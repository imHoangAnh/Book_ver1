using System.Text.RegularExpressions;
using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address.
/// </summary>
public sealed partial class Email : ValueObject
{
    private const int MaxLength = 255;

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Email value object.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <returns>A new Email value object.</returns>
    /// <exception cref="ArgumentException">Thrown when the email is invalid.</exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (email.Length > MaxLength)
            throw new ArgumentException($"Email cannot exceed {MaxLength} characters.", nameof(email));

        if (!EmailRegex().IsMatch(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        return new Email(email.ToLowerInvariant().Trim());
    }

    /// <summary>
    /// Tries to create a new Email value object.
    /// </summary>
    public static bool TryCreate(string email, out Email? result)
    {
        try
        {
            result = Create(email);
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

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
