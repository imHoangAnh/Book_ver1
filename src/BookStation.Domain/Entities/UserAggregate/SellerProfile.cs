using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Seller profile entity.
/// </summary>
public class SellerProfile : Entity<Guid>
{
    /// <summary>
    /// Gets a value indicating whether the seller is approved.
    /// </summary>
    public bool IsApproved { get; private set; }



    /// <summary>
    /// Gets the seller's date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the seller's gender.
    /// </summary>
    public EGender? Gender { get; private set; }

    /// <summary>
    /// Gets the seller's citizen ID number (CCCD).
    /// </summary>
    public string? CitizenIdNumber { get; private set; }

    /// <summary>
    /// Gets the seller's address.
    /// </summary>
    public Address? Address { get; private set; }

    /// <summary>
    /// Gets the date when the seller was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private SellerProfile() { }

    /// <summary>
    /// Creates a new seller profile.
    /// </summary>
    internal static SellerProfile Create(Guid userId)
    {
        return new SellerProfile
        {
            Id = userId, // Uses same ID as User
            IsApproved = false
        };
    }

    /// <summary>
    /// Updates the seller profile information.
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
    /// Approves the seller profile.
    /// </summary>
    public void Approve()
    {
        if (IsApproved)
            return;

        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes seller approval.
    /// </summary>
    public void RevokeApproval()
    {
        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
    }


}
