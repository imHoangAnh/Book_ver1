using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Publisher entity.
/// </summary>
public class Publisher : Entity<long>
{
    /// <summary>
    /// Gets the publisher name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the publisher description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the publisher address.
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// Gets the publisher website.
    /// </summary>
    public string? Website { get; private set; }

    /// <summary>
    /// Gets the publisher logo URL.
    /// </summary>
    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the publisher is active.
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<Book> _books = [];
    public IReadOnlyList<Book> Books => _books.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Publisher() { }

    /// <summary>
    /// Creates a new publisher.
    /// </summary>
    public static Publisher Create(string name, string? description = null, string? address = null, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new Publisher
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Address = address?.Trim(),
            Website = website?.Trim(),
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the publisher.
    /// </summary>
    public void Update(string name, string? description, string? address, string? website, string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Address = address?.Trim();
        Website = website?.Trim();
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the publisher.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the publisher.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the number of books by this publisher.
    /// </summary>
    public int BookCount => _books.Count;
}
