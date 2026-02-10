using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// User entity - Aggregate Root for user management.
/// </summary>
public class User : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Gets the hashed password.
    /// </summary>
    public PasswordHash PasswordHash { get; private set; } = null!;

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName { get; private set; } = null!;

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    public PhoneNumber? Phone { get; private set; }

    /// <summary>
    /// Gets the user's bio/description.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Gets the user's avatar URL.
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Gets the user's address.
    /// </summary>
    public Address? Address { get; private set; }

    /// <summary>
    /// Gets the user's gender.
    /// </summary>
    public EGender? Gender { get; private set; }

    /// <summary>
    /// Gets the user's date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user's email is verified.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Gets the user's account status.
    /// </summary>
    public EUserStatus Status { get; private set; }

    /// <summary>
    /// Gets the reason for ban/suspension.
    /// </summary>
    public string? BanReason { get; private set; }

    /// <summary>
    /// Gets the date when the lockout/suspension ends.
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; private set; }

    // Navigation properties
    public SellerProfile? SellerProfile { get; private set; }
    public AuthorProfile? AuthorProfile { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private User() { }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public static User Create(
        Email email,
        PasswordHash passwordHash,
        string fullName,
        PhoneNumber? phone = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Phone = phone,
            IsVerified = true,
            Status = EUserStatus.Active
        };

        user.AddDomainEvent(new UserCreatedEvent(user));

        return user;
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    public void UpdateProfile(
        string? fullName,
        PhoneNumber? phone,
        string? bio,
        string? avatarUrl,
        Address? address,
        EGender? gender = null,
        DateOnly? dateOfBirth = null)
    {
        // Update FullName only if provided and not empty
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            FullName = fullName;
        }

        // Update other profile fields
        Phone = phone;
        Bio = bio;
        AvatarUrl = avatarUrl;
        Address = address;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserUpdatedEvent(Id, nameof(UpdateProfile)));
    }

    /// <summary>
    /// Updates only the user's avatar URL.
    /// </summary>
    public void UpdateAvatarUrl(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserUpdatedEvent(Id, nameof(UpdateAvatarUrl)));
    }

    /// <summary>
    /// Updates the user's email address.
    /// </summary>
    public void UpdateEmail(Email newEmail)
    {
        if (Email == newEmail)
            return;

        var oldEmail = Email;
        Email = newEmail;
        IsVerified = false; // Require re-verification
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserEmailChangedEvent(Id, oldEmail.Value, newEmail.Value));
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    public void ChangePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserPasswordChangedEvent(Id));
    }

    /// <summary>
    /// Verifies the user's email.
    /// </summary>
    public void Verify()
    {
        if (IsVerified)
            return;

        IsVerified = true;
        Status = EUserStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserVerifiedEvent(Id));
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        if (Status == EUserStatus.Inactive)
            return;

        Status = EUserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeactivatedEvent(Id));
    }

    /// <summary>
    /// Activates the user account.
    /// </summary>
    public void Activate()
    {
        if (Status == EUserStatus.Active)
            return;

        Status = EUserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Bans the user account.
    /// </summary>
    /// <summary>
    /// Bans the user account.
    /// </summary>
    public void Ban(string reason)
    {
        Status = EUserStatus.Banned;
        BanReason = reason;
        LockoutEnd = null; // Permanent ban
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserBannedEvent(Id, reason));
    }

    /// <summary>
    /// Suspends the user account temporarily.
    /// </summary>
    public void Suspend(string reason, DateTime? suspendUntil)
    {
        Status = EUserStatus.Suspended;
        BanReason = reason;
        LockoutEnd = suspendUntil != null ? new DateTimeOffset(suspendUntil.Value) : null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserSuspendedEvent(Id, reason, suspendUntil));
    }

    /// <summary>
    /// Creates a seller profile for this user.
    /// </summary>
    public SellerProfile BecomeASeller()
    {
        if (SellerProfile != null)
            throw new InvalidOperationException("User is already a seller.");

        SellerProfile = SellerProfile.Create(Id);
        
        AddDomainEvent(new UserBecameSellerEvent(Id));
        
        return SellerProfile;
    }



    /// <summary>
    /// Creates an author profile for this user, linking to a catalog author.
    /// </summary>
    public AuthorProfile BecomeAnAuthor(long authorId)
    {
        if (AuthorProfile != null)
            throw new InvalidOperationException("User is already linked to an author.");

        AuthorProfile = AuthorProfile.Create(Id, authorId);
        
        return AuthorProfile;
    }
}
