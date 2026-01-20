using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Inventory entity for stock management.
/// </summary>
public class Inventory : Entity<long>
{
    /// <summary>
    /// Gets the variant ID.
    /// </summary>
    public long VariantId { get; private set; }

    /// <summary>
    /// Gets the current stock quantity.
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// Gets the minimum stock level for low stock alerts.
    /// </summary>
    public int MinStockLevel { get; private set; }

    /// <summary>
    /// Gets the last restocked date.
    /// </summary>
    public DateTime? LastRestockedAt { get; private set; }

    // Navigation properties
    public BookVariant? Variant { get; private set; }

    private readonly List<InventoryReservation> _reservations = [];
    public IReadOnlyList<InventoryReservation> Reservations => _reservations.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Inventory() { }

    /// <summary>
    /// Creates a new inventory record.
    /// </summary>
    public static Inventory Create(long variantId, int initialStock = 0, int minStockLevel = 5)
    {
        if (initialStock < 0)
            throw new ArgumentException("Stock cannot be negative.", nameof(initialStock));

        return new Inventory
        {
            Id = variantId, // Uses same ID as Variant
            VariantId = variantId,
            Stock = initialStock,
            MinStockLevel = minStockLevel,
            LastRestockedAt = initialStock > 0 ? DateTime.UtcNow : null
        };
    }

    /// <summary>
    /// Gets the available stock (excluding reservations).
    /// </summary>
    public int AvailableStock
    {
        get
        {
            var reservedQuantity = _reservations
                .Where(r => r.ExpiresAt > DateTime.UtcNow)
                .Sum(r => r.Quantity);
            return Stock - reservedQuantity;
        }
    }

    /// <summary>
    /// Checks if the specified quantity is available.
    /// </summary>
    public bool IsAvailable(int quantity)
    {
        return AvailableStock >= quantity;
    }

    /// <summary>
    /// Adds stock to inventory.
    /// </summary>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        Stock += quantity;
        LastRestockedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes stock from inventory.
    /// </summary>
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        if (Stock < quantity)
            throw new InvalidOperationException("Insufficient stock.");

        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reserves stock for an order.
    /// </summary>
    public InventoryReservation Reserve(int quantity, TimeSpan duration)
    {
        if (!IsAvailable(quantity))
            throw new InvalidOperationException("Insufficient available stock for reservation.");

        var reservation = InventoryReservation.Create(VariantId, quantity, duration);
        _reservations.Add(reservation);
        UpdatedAt = DateTime.UtcNow;

        return reservation;
    }

    /// <summary>
    /// Releases a reservation.
    /// </summary>
    public void ReleaseReservation(long reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation != null)
        {
            _reservations.Remove(reservation);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Confirms a reservation and deducts from stock.
    /// </summary>
    public void ConfirmReservation(long reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation == null)
            throw new InvalidOperationException("Reservation not found.");

        if (reservation.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Reservation has expired.");

        RemoveStock(reservation.Quantity);
        _reservations.Remove(reservation);
    }

    /// <summary>
    /// Checks if stock is low.
    /// </summary>
    public bool IsLowStock => Stock <= MinStockLevel;

    /// <summary>
    /// Cleans up expired reservations.
    /// </summary>
    public void CleanupExpiredReservations()
    {
        var expiredReservations = _reservations
            .Where(r => r.ExpiresAt <= DateTime.UtcNow)
            .ToList();

        foreach (var reservation in expiredReservations)
        {
            _reservations.Remove(reservation);
        }

        if (expiredReservations.Any())
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
