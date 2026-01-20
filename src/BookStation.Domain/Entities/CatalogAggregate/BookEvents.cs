using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Base event for book-related domain events.
/// </summary>
public abstract class BookBaseEvent : DomainEvent
{
    public long BookId { get; }

    protected BookBaseEvent(long bookId)
    {
        BookId = bookId;
    }
}

/// <summary>
/// Event raised when a new book is created.
/// </summary>
public sealed class BookCreatedEvent : BookBaseEvent
{
    public string Title { get; }
    public string? ISBN { get; }

    public BookCreatedEvent(Book book) : base(book.Id)
    {
        Title = book.Title;
        ISBN = book.ISBN?.Value;
    }
}

/// <summary>
/// Event raised when a book is updated.
/// </summary>
public sealed class BookUpdatedEvent : BookBaseEvent
{
    public BookUpdatedEvent(long bookId) : base(bookId)
    {
    }
}

/// <summary>
/// Event raised when a book is published.
/// </summary>
public sealed class BookPublishedEvent : BookBaseEvent
{
    public BookPublishedEvent(long bookId) : base(bookId)
    {
    }
}

/// <summary>
/// Event raised when book stock is low.
/// </summary>
public sealed class BookLowStockEvent : BookBaseEvent
{
    public long VariantId { get; }
    public int CurrentStock { get; }

    public BookLowStockEvent(long bookId, long variantId, int currentStock) : base(bookId)
    {
        VariantId = variantId;
        CurrentStock = currentStock;
    }
}

/// <summary>
/// Event raised when book is out of stock.
/// </summary>
public sealed class BookOutOfStockEvent : BookBaseEvent
{
    public long VariantId { get; }

    public BookOutOfStockEvent(long bookId, long variantId) : base(bookId)
    {
        VariantId = variantId;
    }
}
