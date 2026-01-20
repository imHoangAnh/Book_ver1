using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Shipper profile entity.
/// </summary>
public class ShipperProfile : Entity<long>
{
    /// <summary>
    /// Gets the organization ID.
    /// </summary>
    public int OrganizationId { get; private set; }

    /// <summary>
    /// Gets the shipper's date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the shipper's gender.
    /// </summary>
    public EGender? Gender { get; private set; }

    /// <summary>
    /// Gets the shipper's citizen ID number (CCCD).
    /// </summary>
    public string? CitizenIdNumber { get; private set; }

    /// <summary>
    /// Gets the shipper's address.
    /// </summary>
    public Address? Address { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the shipper is currently available.
    /// </summary>
    public bool IsAvailable { get; private set; }

    // Navigation properties
    public User? User { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ShipperProfile() { }

    /// <summary>
    /// Creates a new shipper profile.
    /// </summary>
    internal static ShipperProfile Create(long userId, int organizationId)
    {
        return new ShipperProfile
        {
            Id = userId, // Uses same ID as User
            OrganizationId = organizationId,
            IsAvailable = true
        };
    }

    /// <summary>
    /// Updates the shipper profile information.
    /// </summary>
    public void UpdateProfile(
        DateTime? dateOfBirth,
        EGender? gender,
        string? citizenIdNumber,
        Address? address)
    {
        DateOfBirth = dateOfBirth;
        Gender = gender;
        CitizenIdNumber = citizenIdNumber;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the shipper's availability status.
    /// </summary>
    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }
}
