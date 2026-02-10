namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Junction entity for Book-Author many-to-many relationship.
/// This represents main authors only. Translators and co-authors are stored as strings in Book.
/// </summary>
public class BookAuthor
{
    /// <summary>
    /// Gets the book ID.
    /// </summary>
    public long BookId { get; private set; }

    /// <summary>
    /// Gets the author ID.
    /// </summary>
    public long AuthorId { get; private set; }

    /// <summary>
    /// Gets the display order (for ordering multiple authors).
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public Book? Book { get; private set; }
    public Author? Author { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BookAuthor() { }

    /// <summary>
    /// Creates a new book-author association.
    /// </summary>
    internal static BookAuthor Create(long bookId, long authorId, int displayOrder = 1)
    {
        return new BookAuthor
        {
            BookId = bookId,
            AuthorId = authorId,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the display order.
    /// </summary>
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
