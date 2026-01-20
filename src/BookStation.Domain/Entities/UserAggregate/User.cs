using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// User entity - Aggregate Root for user management.
/// </summary>
public class User : AggregateRoot<long>
{
    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Gets the hashed password.
    /// </summary>
    public string PasswordHash { get; private set; } = null!;

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string? FullName { get; private set; }

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    public PhoneNumber? Phone { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user's email is verified.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Gets the user's account status.
    /// </summary>
    public EUserStatus Status { get; private set; }

    // Navigation properties
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    public SellerProfile? SellerProfile { get; private set; }
    public ShipperProfile? ShipperProfile { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private User() { }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public static User Create(
        Email email,
        string passwordHash,
        string? fullName = null,
        PhoneNumber? phone = null)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Phone = phone,
            IsVerified = false,
            Status = EUserStatus.Pending
        };

        user.AddDomainEvent(new UserCreatedEvent(user));

        return user;
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    public void UpdateProfile(string? fullName, PhoneNumber? phone)
    {
        FullName = fullName;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserUpdatedEvent(Id, nameof(UpdateProfile)));
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
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
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
    /// Assigns a role to the user.
    /// </summary>
    public void AssignRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return;

        _userRoles.Add(UserRole.Create(Id, role.Id));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(int roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            UpdatedAt = DateTime.UtcNow;
        }
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
    public void Ban(string reason)
    {
        Status = EUserStatus.Banned;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserBannedEvent(Id, reason));
    }

    /// <summary>
    /// Creates a seller profile for this user.
    /// </summary>
    public SellerProfile BecomeASeller(int? organizationId = null)
    {
        if (SellerProfile != null)
            throw new InvalidOperationException("User is already a seller.");

        SellerProfile = SellerProfile.Create(Id, organizationId);
        
        AddDomainEvent(new UserBecameSellerEvent(Id));
        
        return SellerProfile;
    }

    /// <summary>
    /// Creates a shipper profile for this user.
    /// </summary>
    public ShipperProfile BecomeAShipper(int organizationId)
    {
        if (ShipperProfile != null)
            throw new InvalidOperationException("User is already a shipper.");

        ShipperProfile = ShipperProfile.Create(Id, organizationId);
        
        return ShipperProfile;
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => ur.Role?.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase) == true);
    }
}
