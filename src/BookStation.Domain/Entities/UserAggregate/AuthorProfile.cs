using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Author profile entity for users who are verified authors.
/// Links a user account to an Author in the catalog.
/// </summary>
public class AuthorProfile : Entity<Guid>
{
    /// <summary>
    /// Gets the linked author ID from the catalog.
    /// </summary>
    public long AuthorId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the author identity is verified (blue tick).
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Gets the date when the author was verified.
    /// </summary>
    public DateTime? VerifiedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private AuthorProfile() { }

    /// <summary>
    /// Creates a new author profile.
    /// </summary>
    internal static AuthorProfile Create(Guid userId, long authorId)
    {
        return new AuthorProfile
        {
            Id = userId, // Uses same ID as User
            AuthorId = authorId,
            IsVerified = false
        };
    }

    /// <summary>
    /// Verifies the author profile (grants blue tick). Admin only.
    /// </summary>
    public void Verify()
    {
        if (IsVerified)
            return;

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes the author verification. Admin only.
    /// </summary>
    public void RevokeVerification()
    {
        if (!IsVerified)
            return;

        IsVerified = false;
        VerifiedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the linked author ID.
    /// </summary>
    public void UpdateAuthorId(long authorId)
    {
        AuthorId = authorId;
        IsVerified = false; // Require re-verification when changing author
        VerifiedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
