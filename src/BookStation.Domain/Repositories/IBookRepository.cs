using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.CatalogAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Book aggregate.
/// </summary>
public interface IBookRepository : IRepository<Book, long>
{
    /// <summary>
    /// Gets a book by ISBN.
    /// </summary>
    Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an ISBN is already in use.
    /// </summary>
    Task<bool> IsISBNUniqueAsync(string isbn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book with its variants.
    /// </summary>
    Task<Book?> GetWithVariantsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book with all related data.
    /// </summary>
    Task<Book?> GetWithAllDetailsAsync(long id, CancellationToken cancellationToken = default);
}

