namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Junction entity for Book-Category many-to-many relationship.
/// </summary>
public class BookCategory
{
    /// <summary>
    /// Gets the book ID.
    /// </summary>
    public long BookId { get; private set; }

    /// <summary>
    /// Gets the category ID.
    /// </summary>
    public int CategoryId { get; private set; }

    // Navigation properties
    public Book? Book { get; private set; }
    public Category? Category { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BookCategory() { }

    /// <summary>
    /// Creates a new book-category association.
    /// </summary>
    internal static BookCategory Create(long bookId, int categoryId)
    {
        return new BookCategory
        {
            BookId = bookId,
            CategoryId = categoryId
        };
    }
}
