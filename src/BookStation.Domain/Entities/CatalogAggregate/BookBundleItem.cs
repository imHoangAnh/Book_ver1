using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// BookBundleItem entity - represents a book in a bundle.
/// </summary>
public class BookBundleItem : Entity<long>
{
    /// <summary>
    /// Gets the bundle ID.
    /// </summary>
    public long BundleId { get; private set; }

    /// <summary>
    /// Gets the book ID.
    /// </summary>
    public long BookId { get; private set; }

    /// <summary>
    /// Gets the specific variant ID (optional).
    /// If null, any variant of the book can be selected.
    /// </summary>
    public long? VariantId { get; private set; }

    /// <summary>
    /// Gets the display order in the bundle.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public BookBundle? Bundle { get; private set; }
    public Book? Book { get; private set; }
    public BookVariant? Variant { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BookBundleItem() { }

    /// <summary>
    /// Creates a new bundle item.
    /// </summary>
    internal static BookBundleItem Create(long bundleId, long bookId, long? variantId = null, int displayOrder = 0)
    {
        return new BookBundleItem
        {
            BundleId = bundleId,
            BookId = bookId,
            VariantId = variantId,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the display order.
    /// </summary>
    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets or updates the specific variant.
    /// </summary>
    public void SetVariant(long? variantId)
    {
        VariantId = variantId;
        UpdatedAt = DateTime.UtcNow;
    }
}
