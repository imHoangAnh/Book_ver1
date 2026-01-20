using BookStation.Application.Users.Commands;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BookStation.Infrastructure.Services;

/// <summary>
/// Password hashing service using PBKDF2.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8;
    private const int HashSize = 256 / 8;
    private const int Iterations = 100000;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // Generate salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash password
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Combine salt and hash
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        if (string.IsNullOrEmpty(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash));

        byte[] hashBytes = Convert.FromBase64String(passwordHash);

        // Extract salt
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Hash the password with the extracted salt
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Compare hashes
        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
                return false;
        }

        return true;
    }
}
