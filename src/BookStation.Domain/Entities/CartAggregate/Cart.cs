using BookStation.Core.SharedKernel;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.CartAggregate;

/// <summary>
/// Cart entity - Aggregate Root for shopping cart management.
/// </summary>
public class Cart : AggregateRoot<long>
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Gets when the cart was last updated.
    /// </summary>
    public DateTime LastActivityAt { get; private set; }

    // Navigation properties
    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Cart() { }

    /// <summary>
    /// Creates a new cart for a user.
    /// </summary>
    public static Cart Create(long userId)
    {
        return new Cart
        {
            UserId = userId,
            LastActivityAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds an item to the cart.
    /// </summary>
    public CartItem AddItem(long variantId, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        var existingItem = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = CartItem.Create(Id, variantId, quantity, unitPrice);
            _items.Add(item);
        }

        LastActivityAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return _items.First(i => i.VariantId == variantId);
    }

    /// <summary>
    /// Updates the quantity of an item.
    /// </summary>
    public void UpdateItemQuantity(long variantId, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveItem(variantId);
            return;
        }

        var item = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (item != null)
        {
            item.UpdateQuantity(quantity);
            LastActivityAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public void RemoveItem(long variantId)
    {
        var item = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (item != null)
        {
            _items.Remove(item);
            LastActivityAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        LastActivityAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the total amount.
    /// </summary>
    public Money Total => _items.Aggregate(
        Money.Zero(),
        (sum, item) => sum + item.Subtotal);

    /// <summary>
    /// Gets the total number of items.
    /// </summary>
    public int ItemCount => _items.Sum(i => i.Quantity);

    /// <summary>
    /// Checks if the cart is empty.
    /// </summary>
    public bool IsEmpty => !_items.Any();
}
