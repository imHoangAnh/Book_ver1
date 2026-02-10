using BookStation.Domain.Entities.CatalogAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Inventory management.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Gets inventory by variant ID.
    /// </summary>
    Task<Inventory?> GetByVariantIdAsync(long variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the price of a variant.
    /// </summary>
    Task<decimal> GetVariantPriceAsync(long variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets variant information (book title and variant name).
    /// </summary>
    Task<VariantInfo?> GetVariantInfoAsync(long variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an inventory record.
    /// </summary>
    void Update(Inventory inventory);

    /// <summary>
    /// Gets low stock items.
    /// </summary>
    Task<IEnumerable<Inventory>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Variant information for display.
/// </summary>
public record VariantInfo(
    long VariantId,
    string BookTitle,
    string VariantName
);
