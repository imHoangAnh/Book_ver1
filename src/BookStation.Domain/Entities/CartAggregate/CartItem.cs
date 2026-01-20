using BookStation.Core.SharedKernel;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.CartAggregate;

/// <summary>
/// Cart item entity.
/// </summary>
public class CartItem : Entity<long>
{
    /// <summary>
    /// Gets the cart ID.
    /// </summary>
    public long CartId { get; private set; }

    /// <summary>
    /// Gets the variant ID.
    /// </summary>
    public long VariantId { get; private set; }

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price (for display, may change).
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    /// <summary>
    /// Gets the subtotal.
    /// </summary>
    public Money Subtotal => UnitPrice * Quantity;

    // Navigation properties
    public Cart? Cart { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private CartItem() { }

    /// <summary>
    /// Creates a new cart item.
    /// </summary>
    internal static CartItem Create(long cartId, long variantId, int quantity, Money unitPrice)
    {
        return new CartItem
        {
            CartId = cartId,
            VariantId = variantId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    /// <summary>
    /// Updates the quantity.
    /// </summary>
    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the unit price.
    /// </summary>
    public void UpdatePrice(Money newPrice)
    {
        UnitPrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
