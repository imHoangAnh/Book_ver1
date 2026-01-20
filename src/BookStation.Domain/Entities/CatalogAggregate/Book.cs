using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Book entity - Aggregate Root for book management.
/// </summary>
public class Book : AggregateRoot<long>
{
    /// <summary>
    /// Gets the book title.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Gets the ISBN.
    /// </summary>
    public ISBN? ISBN { get; private set; }

    /// <summary>
    /// Gets the book description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the language of the book.
    /// </summary>
    public string? Language { get; private set; }

    /// <summary>
    /// Gets the publication year.
    /// </summary>
    public int? PublishYear { get; private set; }

    /// <summary>
    /// Gets the publisher ID.
    /// </summary>
    public long? PublisherId { get; private set; }

    /// <summary>
    /// Gets the book status.
    /// </summary>
    public EBookStatus Status { get; private set; }

    /// <summary>
    /// Gets the cover image URL.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Gets the number of pages.
    /// </summary>
    public int? PageCount { get; private set; }

    // Navigation properties
    public Publisher? Publisher { get; private set; }

    private readonly List<BookVariant> _variants = [];
    public IReadOnlyList<BookVariant> Variants => _variants.AsReadOnly();

    private readonly List<BookAuthor> _bookAuthors = [];
    public IReadOnlyList<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    private readonly List<BookCategory> _bookCategories = [];
    public IReadOnlyList<BookCategory> BookCategories => _bookCategories.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Book() { }

    /// <summary>
    /// Creates a new book.
    /// </summary>
    public static Book Create(
        string title,
        ISBN? isbn = null,
        string? description = null,
        string? language = null,
        int? publishYear = null,
        long? publisherId = null,
        string? coverImageUrl = null,
        int? pageCount = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        var book = new Book
        {
            Title = title.Trim(),
            ISBN = isbn,
            Description = description?.Trim(),
            Language = language?.Trim(),
            PublishYear = publishYear,
            PublisherId = publisherId,
            CoverImageUrl = coverImageUrl,
            PageCount = pageCount,
            Status = EBookStatus.Draft
        };

        book.AddDomainEvent(new BookCreatedEvent(book));

        return book;
    }

    /// <summary>
    /// Updates the book information.
    /// </summary>
    public void Update(
        string title,
        ISBN? isbn,
        string? description,
        string? language,
        int? publishYear,
        long? publisherId,
        string? coverImageUrl,
        int? pageCount)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Title = title.Trim();
        ISBN = isbn;
        Description = description?.Trim();
        Language = language?.Trim();
        PublishYear = publishYear;
        PublisherId = publisherId;
        CoverImageUrl = coverImageUrl;
        PageCount = pageCount;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookUpdatedEvent(Id));
    }

    /// <summary>
    /// Adds a variant to the book.
    /// </summary>
    public BookVariant AddVariant(string variantName, Money price)
    {
        if (_variants.Any(v => v.VariantName.Equals(variantName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Variant '{variantName}' already exists.");

        var variant = BookVariant.Create(Id, variantName, price);
        _variants.Add(variant);
        UpdatedAt = DateTime.UtcNow;

        return variant;
    }

    /// <summary>
    /// Removes a variant from the book.
    /// </summary>
    public void RemoveVariant(long variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant != null)
        {
            _variants.Remove(variant);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds an author to the book.
    /// </summary>
    public void AddAuthor(long authorId, EAuthorRole role = EAuthorRole.Author, int displayOrder = 1)
    {
        if (_bookAuthors.Any(ba => ba.AuthorId == authorId))
            return;

        _bookAuthors.Add(BookAuthor.Create(Id, authorId, role, displayOrder));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an author from the book.
    /// </summary>
    public void RemoveAuthor(long authorId)
    {
        var bookAuthor = _bookAuthors.FirstOrDefault(ba => ba.AuthorId == authorId);
        if (bookAuthor != null)
        {
            _bookAuthors.Remove(bookAuthor);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds a category to the book.
    /// </summary>
    public void AddCategory(int categoryId)
    {
        if (_bookCategories.Any(bc => bc.CategoryId == categoryId))
            return;

        _bookCategories.Add(BookCategory.Create(Id, categoryId));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a category from the book.
    /// </summary>
    public void RemoveCategory(int categoryId)
    {
        var bookCategory = _bookCategories.FirstOrDefault(bc => bc.CategoryId == categoryId);
        if (bookCategory != null)
        {
            _bookCategories.Remove(bookCategory);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Publishes the book (makes it active).
    /// </summary>
    public void Publish()
    {
        if (Status == EBookStatus.Active)
            return;

        if (!HasAnyVariant())
            throw new InvalidOperationException("Cannot publish a book without variants.");

        Status = EBookStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookPublishedEvent(Id));
    }

    /// <summary>
    /// Unpublishes the book (makes it inactive).
    /// </summary>
    public void Unpublish()
    {
        Status = EBookStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the book as out of stock.
    /// </summary>
    public void MarkOutOfStock()
    {
        Status = EBookStatus.OutOfStock;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Discontinues the book.
    /// </summary>
    public void Discontinue()
    {
        Status = EBookStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the minimum price among all variants.
    /// </summary>
    public Money? GetMinPrice()
    {
        if (!_variants.Any())
            return null;

        return _variants.Min(v => v.Price);
    }

    /// <summary>
    /// Gets the maximum price among all variants.
    /// </summary>
    public Money? GetMaxPrice()
    {
        if (!_variants.Any())
            return null;

        return _variants.Max(v => v.Price);
    }

    private bool HasAnyVariant() => _variants.Any();
}
