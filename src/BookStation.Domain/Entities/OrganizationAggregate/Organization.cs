using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;

namespace BookStation.Domain.Entities.OrganizationAggregate;

/// <summary>
/// Organization entity for bookstores and publishers.
/// </summary>
public class Organization : Entity<int>
{
    /// <summary>
    /// Gets the organization name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the organization description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the organization type.
    /// </summary>
    public EOrganizationType Type { get; private set; }

    /// <summary>
    /// Gets the logo URL.
    /// </summary>
    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Gets the tax code.
    /// </summary>
    public string? TaxCode { get; private set; }

    /// <summary>
    /// Gets the publisher ID if this is a publisher organization.
    /// </summary>
    public long? PublisherId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the organization is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Organization() { }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    public static Organization Create(
        string name,
        EOrganizationType type,
        string? description = null,
        string? logoUrl = null,
        string? taxCode = null,
        long? publisherId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new Organization
        {
            Name = name.Trim(),
            Type = type,
            Description = description?.Trim(),
            LogoUrl = logoUrl,
            TaxCode = taxCode?.Trim(),
            PublisherId = publisherId,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the organization.
    /// </summary>
    public void Update(
        string name,
        string? description,
        string? logoUrl,
        string? taxCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        LogoUrl = logoUrl;
        TaxCode = taxCode?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the organization.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the organization.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
