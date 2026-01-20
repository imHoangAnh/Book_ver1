using BookStation.Domain.Enums;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Junction entity for Book-Author many-to-many relationship.
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
    /// Gets the author's role in this book.
    /// </summary>
    public EAuthorRole Role { get; private set; }

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
    internal static BookAuthor Create(long bookId, long authorId, EAuthorRole role = EAuthorRole.Author, int displayOrder = 1)
    {
        return new BookAuthor
        {
            BookId = bookId,
            AuthorId = authorId,
            Role = role,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the role and display order.
    /// </summary>
    public void Update(EAuthorRole role, int displayOrder)
    {
        Role = role;
        DisplayOrder = displayOrder;
    }
}
