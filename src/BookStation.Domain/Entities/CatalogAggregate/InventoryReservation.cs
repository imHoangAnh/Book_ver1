using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Inventory reservation entity for temporary stock holds.
/// </summary>
public class InventoryReservation : Entity<long>
{
    /// <summary>
    /// Gets the variant ID.
    /// </summary>
    public long VariantId { get; private set; }

    /// <summary>
    /// Gets the reserved quantity.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the expiration time.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the reservation is expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    // Navigation properties
    public Inventory? Inventory { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private InventoryReservation() { }

    /// <summary>
    /// Creates a new inventory reservation.
    /// </summary>
    internal static InventoryReservation Create(long variantId, int quantity, TimeSpan duration)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        // Default reservation duration is 15 minutes
        if (duration <= TimeSpan.Zero)
            duration = TimeSpan.FromMinutes(15);

        return new InventoryReservation
        {
            VariantId = variantId,
            Quantity = quantity,
            ExpiresAt = DateTime.UtcNow.Add(duration)
        };
    }

    /// <summary>
    /// Extends the reservation time.
    /// </summary>
    public void Extend(TimeSpan additionalDuration)
    {
        ExpiresAt = ExpiresAt.Add(additionalDuration);
        UpdatedAt = DateTime.UtcNow;
    }
}
