using BookStation.Application.Services;
using BookStation.Domain.ValueObjects;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BookStation.Infrastructure.Services;

/// <summary>
/// Password hashing service using PBKDF2.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // 2^12 iterations (~4096). Tăng số này nếu muốn chậm hơn (an toàn hơn)

    /// <inheritdoc />
    public PasswordHash HashPassword(Password password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        // BCrypt tự động handle salt, không cần generate thủ công
        string hash = BCrypt.Net.BCrypt.HashPassword(password.Value, WorkFactor);
        
        return PasswordHash.FromHash(hash);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, PasswordHash passwordHash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        if (passwordHash is null)
            throw new ArgumentNullException(nameof(passwordHash));

        return VerifyPasswordInternal(password, passwordHash.Value);
    }

    /// <inheritdoc />
    public bool VerifyPassword(Password password, PasswordHash passwordHash)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        if (passwordHash is null)
            throw new ArgumentNullException(nameof(passwordHash));

        return VerifyPasswordInternal(password.Value, passwordHash.Value);
    }

    private bool VerifyPasswordInternal(string password, string storedHash)
    {
        try 
        {
            // BCrypt.Verify tự động extract salt từ storedHash và so sánh
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Nếu hash trong DB bị lỗi định dạng hoặc là hash cũ (PBKDF2) không tương thích
            return false;
        }
    }
}

