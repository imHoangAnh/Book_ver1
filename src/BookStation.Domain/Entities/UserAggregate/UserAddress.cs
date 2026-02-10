using BookStation.Core.SharedKernel;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Entity representing a user's shipping/billing address.
/// A user can have multiple addresses.
/// </summary>
public class UserAddress : Entity<int>
{
    /// <summary>
    /// Gets the user ID this address belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the address label (e.g., "Home", "Work", "Office").
    /// </summary>
    public string Label { get; private set; } = "Home";

    /// <summary>
    /// Gets the address details.
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets whether this is the default address.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Gets the recipient name for this address.
    /// </summary>
    public string? RecipientName { get; private set; }

    /// <summary>
    /// Gets the recipient phone number for this address.
    /// </summary>
    public string? RecipientPhone { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private UserAddress() { }

    /// <summary>
    /// Creates a new user address.
    /// </summary>
    public static UserAddress Create(
        Guid userId,
        Address address,
        string? label = "Home",
        bool isDefault = false,
        string? recipientName = null,
        string? recipientPhone = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new UserAddress
        {
            UserId = userId,
            Address = address ?? throw new ArgumentNullException(nameof(address)),
            Label = string.IsNullOrWhiteSpace(label) ? "Home" : label.Trim(),
            IsDefault = isDefault,
            RecipientName = recipientName?.Trim(),
            RecipientPhone = recipientPhone?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Sets this address as the default.
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes default status from this address.
    /// </summary>
    public void RemoveDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the address details.
    /// </summary>
    public void Update(
        Address address,
        string? label = null,
        string? recipientName = null,
        string? recipientPhone = null)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        
        if (!string.IsNullOrWhiteSpace(label))
            Label = label.Trim();
        
        RecipientName = recipientName?.Trim();
        RecipientPhone = recipientPhone?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
