using BookStation.Core.SharedKernel;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.OrderAggregate;

/// <summary>
/// Order item entity.
/// </summary>
public class OrderItem : Entity<long>
{
    /// <summary>
    /// Gets the order ID.
    /// </summary>
    public long OrderId { get; private set; }

    /// <summary>
    /// Gets the variant ID.
    /// </summary>
    public long VariantId { get; private set; }

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price at the time of order.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    /// <summary>
    /// Gets the subtotal (quantity * unit price).
    /// </summary>
    public Money Subtotal { get; private set; } = null!;

    /// <summary>
    /// Gets the book title snapshot.
    /// </summary>
    public string BookTitle { get; private set; } = null!;

    /// <summary>
    /// Gets the variant name snapshot.
    /// </summary>
    public string VariantName { get; private set; } = null!;

    // Navigation properties
    public Order? Order { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private OrderItem() { }

    /// <summary>
    /// Creates a new order item.
    /// </summary>
    internal static OrderItem Create(
        long orderId,
        long variantId,
        int quantity,
        Money unitPrice,
        string bookTitle,
        string variantName)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        return new OrderItem
        {
            OrderId = orderId,
            VariantId = variantId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = unitPrice * quantity,
            BookTitle = bookTitle,
            VariantName = variantName
        };
    }

    /// <summary>
    /// Updates the quantity.
    /// </summary>
    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(newQuantity));

        Quantity = newQuantity;
        Subtotal = UnitPrice * Quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
