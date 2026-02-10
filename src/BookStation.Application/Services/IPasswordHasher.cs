using BookStation.Domain.ValueObjects;

namespace BookStation.Application.Services;

/// <summary>
/// Password hashing service interface.
/// Provides methods for securely hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a validated password.
    /// </summary>
    /// <param name="password">The validated Password value object.</param>
    /// <returns>A PasswordHash value object containing the hashed password.</returns>
    PasswordHash HashPassword(Password password);

    /// <summary>
    /// Hashes a raw password string. Use this only when Password validation is done elsewhere.
    /// </summary>
    /// <param name="password">The plain-text password string.</param>
    /// <returns>A PasswordHash value object containing the hashed password.</returns>
    PasswordHash HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    bool VerifyPassword(string password, PasswordHash passwordHash);

    /// <summary>
    /// Verifies a validated Password against a stored hash.
    /// </summary>
    /// <param name="password">The Password value object to verify.</param>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    bool VerifyPassword(Password password, PasswordHash passwordHash);
}
