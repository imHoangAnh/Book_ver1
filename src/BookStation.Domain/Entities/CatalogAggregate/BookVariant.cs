using BookStation.Core.SharedKernel;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Book variant entity (e.g., Hardcover, Paperback, Ebook).
/// </summary>
public class BookVariant : Entity<long>
{
    /// <summary>
    /// Gets the book ID.
    /// </summary>
    public long BookId { get; private set; }

    /// <summary>
    /// Gets the variant name.
    /// </summary>
    public string VariantName { get; private set; } = null!;

    /// <summary>
    /// Gets the price.
    /// </summary>
    public Money Price { get; private set; } = null!;

    /// <summary>
    /// Gets the original price (before discount).
    /// </summary>
    public Money? OriginalPrice { get; private set; }

    /// <summary>
    /// Gets the weight in grams.
    /// </summary>
    public int? WeightGrams { get; private set; }

    /// <summary>
    /// Gets the SKU (Stock Keeping Unit).
    /// </summary>
    public string? SKU { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this variant is available.
    /// </summary>
    public bool IsAvailable { get; private set; }

    // Navigation properties
    public Book? Book { get; private set; }
    public Inventory? Inventory { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BookVariant() { }

    /// <summary>
    /// Creates a new book variant.
    /// </summary>
    internal static BookVariant Create(long bookId, string variantName, Money price)
    {
        if (string.IsNullOrWhiteSpace(variantName))
            throw new ArgumentException("Variant name cannot be empty.", nameof(variantName));

        return new BookVariant
        {
            BookId = bookId,
            VariantName = variantName.Trim(),
            Price = price,
            IsAvailable = true
        };
    }

    /// <summary>
    /// Updates the variant information.
    /// </summary>
    public void Update(string variantName, Money price, Money? originalPrice = null, int? weightGrams = null, string? sku = null)
    {
        if (string.IsNullOrWhiteSpace(variantName))
            throw new ArgumentException("Variant name cannot be empty.", nameof(variantName));

        VariantName = variantName.Trim();
        Price = price;
        OriginalPrice = originalPrice;
        WeightGrams = weightGrams;
        SKU = sku?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the price.
    /// </summary>
    public void UpdatePrice(Money newPrice, Money? originalPrice = null)
    {
        OriginalPrice = originalPrice ?? Price;
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the availability status.
    /// </summary>
    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the variant has a discount.
    /// </summary>
    public bool HasDiscount => OriginalPrice != null && OriginalPrice > Price;

    /// <summary>
    /// Gets the discount percentage.
    /// </summary>
    public decimal GetDiscountPercentage()
    {
        if (!HasDiscount || OriginalPrice == null)
            return 0;

        return Math.Round((1 - Price.Amount / OriginalPrice.Amount) * 100, 2);
    }
}
