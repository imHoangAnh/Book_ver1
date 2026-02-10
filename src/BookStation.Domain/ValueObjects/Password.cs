using System.Text.RegularExpressions;
using BookStation.Core.SharedKernel;
using BookStation.Core.Exceptions;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object representing a plain-text password with validation rules.
/// This is used for password validation before hashing.
/// </summary>
public sealed partial class Password : ValueObject
{
    /// <summary>
    /// Minimum required password length.
    /// </summary>
    public const int MinLength = 8;

    /// <summary>
    /// Maximum allowed password length.
    /// </summary>
    public const int MaxLength = 128;

    /// <summary>
    /// Gets the password value.
    /// Note: This should only be used temporarily for hashing and then discarded.
    /// </summary>
    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Password value object with full validation.
    /// </summary>
    /// <param name="password">The plain-text password.</param>
    /// <returns>A new Password value object.</returns>
    /// <exception cref="ValidationException">Thrown when the password doesn't meet requirements.</exception>
    public static Password Create(string password)
    {
        var errors = Validate(password);
        
        if (errors.Count > 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Password", errors.ToArray() }
            });
        }

        return new Password(password);
    }

    /// <summary>
    /// Tries to create a new Password value object.
    /// </summary>
    /// <param name="password">The plain-text password.</param>
    /// <param name="result">The created Password if successful.</param>
    /// <returns>True if password meets all requirements, false otherwise.</returns>
    public static bool TryCreate(string password, out Password? result)
    {
        try
        {
            result = Create(password);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Validates a password and returns a list of validation errors.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>List of validation error messages, empty if valid.</returns>
    public static List<string> Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (password.Length < MinLength)
        {
            errors.Add($"Password must be at least {MinLength} characters long.");
        }

        if (password.Length > MaxLength)
        {
            errors.Add($"Password cannot exceed {MaxLength} characters.");
        }

        if (!HasUppercaseLetter().IsMatch(password))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!HasLowercaseLetter().IsMatch(password))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (!HasDigit().IsMatch(password))
        {
            errors.Add("Password must contain at least one digit.");
        }

        // Optional: require special character for stronger passwords
        // if (!HasSpecialCharacter().IsMatch(password))
        // {
        //     errors.Add("Password must contain at least one special character (!@#$%^&*(),.?\":{}|<>).");
        // }

        return errors;
    }

    /// <summary>
    /// Checks if a password meets all requirements without throwing exceptions.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>True if password is valid, false otherwise.</returns>
    public static bool IsValid(string password)
    {
        return Validate(password).Count == 0;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => "********"; // Never expose password in logs

    public static implicit operator string(Password password) => password.Value;

    [GeneratedRegex(@"[A-Z]", RegexOptions.Compiled)]
    private static partial Regex HasUppercaseLetter();

    [GeneratedRegex(@"[a-z]", RegexOptions.Compiled)]
    private static partial Regex HasLowercaseLetter();

    [GeneratedRegex(@"[0-9]", RegexOptions.Compiled)]
    private static partial Regex HasDigit();

    [GeneratedRegex(@"[!@#$%^&*()\-_=+\[\]{};':""\\|,.<>\/?`~]", RegexOptions.Compiled)]
    private static partial Regex HasSpecialCharacter();
}
