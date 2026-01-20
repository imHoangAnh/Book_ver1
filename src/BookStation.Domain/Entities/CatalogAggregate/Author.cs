using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Author entity.
/// </summary>
public class Author : Entity<long>
{
    /// <summary>
    /// Gets the author's full name.
    /// </summary>
    public string FullName { get; private set; } = null!;

    /// <summary>
    /// Gets the author's biography.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Gets the date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the date of death (if applicable).
    /// </summary>
    public DateTime? DiedDate { get; private set; }

    /// <summary>
    /// Gets the author's address.
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// Gets the author's country.
    /// </summary>
    public string? Country { get; private set; }

    /// <summary>
    /// Gets the user ID if the author has an account.
    /// </summary>
    public long? UserId { get; private set; }

    /// <summary>
    /// Gets the author's photo URL.
    /// </summary>
    public string? PhotoUrl { get; private set; }

    // Navigation properties
    private readonly List<BookAuthor> _bookAuthors = [];
    public IReadOnlyList<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Author() { }

    /// <summary>
    /// Creates a new author.
    /// </summary>
    public static Author Create(
        string fullName,
        string? bio = null,
        DateTime? dateOfBirth = null,
        string? country = null,
        long? userId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        return new Author
        {
            FullName = fullName.Trim(),
            Bio = bio?.Trim(),
            DateOfBirth = dateOfBirth,
            Country = country?.Trim(),
            UserId = userId
        };
    }

    /// <summary>
    /// Updates the author information.
    /// </summary>
    public void Update(
        string fullName,
        string? bio,
        DateTime? dateOfBirth,
        DateTime? diedDate,
        string? address,
        string? country,
        string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        FullName = fullName.Trim();
        Bio = bio?.Trim();
        DateOfBirth = dateOfBirth;
        DiedDate = diedDate;
        Address = address?.Trim();
        Country = country?.Trim();
        PhotoUrl = photoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Links this author to a user account.
    /// </summary>
    public void LinkToUser(long userId)
    {
        UserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the number of books by this author.
    /// </summary>
    public int BookCount => _bookAuthors.Count;
}
