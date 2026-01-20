using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.CatalogAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Book aggregate.
/// </summary>
public interface IBookRepository : IWriteOnlyRepository<Book, long>
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

/// <summary>
/// Repository interface for Inventory operations.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Gets inventory by variant ID.
    /// </summary>
    Task<Inventory?> GetByVariantIdAsync(long variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory with reservations.
    /// </summary>
    Task<Inventory?> GetWithReservationsAsync(long variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new inventory record.
    /// </summary>
    Task AddAsync(Inventory inventory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an inventory record.
    /// </summary>
    void Update(Inventory inventory);

    /// <summary>
    /// Gets the price for a variant.
    /// </summary>
    Task<decimal> GetVariantPriceAsync(long variantId, CancellationToken cancellationToken = default);
}
