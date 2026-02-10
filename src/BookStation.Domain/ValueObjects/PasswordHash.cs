using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object representing a hashed password.
/// This is the result of hashing a Password value object.
/// </summary>
public sealed class PasswordHash : ValueObject
{
    /// <summary>
    /// Gets the hashed password value (BCrypt format).
    /// </summary>
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a PasswordHash from an existing hash string.
    /// Used when loading from database or after hashing.
    /// </summary>
    /// <param name="hash">The BCrypt hash string.</param>
    /// <returns>A PasswordHash value object.</returns>
    public static PasswordHash FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(hash));

        return new PasswordHash(hash);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => "********"; // Never expose hash in logs

    public static implicit operator string(PasswordHash hash) => hash.Value;
}
