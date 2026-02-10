using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// BookBundle entity - represents a bundle of books (BundleSet or Combo).
/// </summary>
public class BookBundle : AggregateRoot<long>
{
    /// <summary>
    /// Gets the seller ID who created this bundle.
    /// </summary>
    public Guid SellerId { get; private set; }

    /// <summary>
    /// Gets the bundle type.
    /// </summary>
    public EBundleType BundleType { get; private set; }

    /// <summary>
    /// Gets the bundle name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the bundle description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the cover image URL.
    /// </summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>
    /// Gets the fixed price for BundleSet type.
    /// For Combo type, this is null (price is calculated from selected books).
    /// </summary>
    public Money? Price { get; private set; }

    /// <summary>
    /// Gets the discount type (Percent or FixedAmount).
    /// Used for Combo type to apply discount on total price.
    /// </summary>
    public EDiscountType? DiscountType { get; private set; }

    /// <summary>
    /// Gets the discount value.
    /// </summary>
    public decimal? DiscountValue { get; private set; }

    /// <summary>
    /// Gets the required quantity for Combo type.
    /// Customer must select this many books to get the discount.
    /// </summary>
    public int? RequiredQuantity { get; private set; }

    /// <summary>
    /// Gets the start date of the bundle validity.
    /// </summary>
    public DateTime? StartDate { get; private set; }

    /// <summary>
    /// Gets the end date of the bundle validity.
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the bundle is active.
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<BookBundleItem> _items = [];
    public IReadOnlyList<BookBundleItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BookBundle() { }

    /// <summary>
    /// Creates a new BundleSet (fixed set of books).
    /// </summary>
    public static BookBundle CreateBundleSet(
        Guid sellerId,
        string name,
        Money price,
        string? description = null,
        string? coverImageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new BookBundle
        {
            SellerId = sellerId,
            BundleType = EBundleType.BundleSet,
            Name = name.Trim(),
            Description = description?.Trim(),
            CoverImageUrl = coverImageUrl,
            Price = price,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates a new Combo (promotional bundle with customer choice).
    /// </summary>
    public static BookBundle CreateCombo(
        Guid sellerId,
        string name,
        int requiredQuantity,
        EDiscountType discountType,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        string? coverImageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (requiredQuantity < 1)
            throw new ArgumentException("Required quantity must be at least 1.", nameof(requiredQuantity));

        if (discountValue <= 0)
            throw new ArgumentException("Discount value must be positive.", nameof(discountValue));

        if (discountType == EDiscountType.Percent && discountValue > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%.", nameof(discountValue));

        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));

        return new BookBundle
        {
            SellerId = sellerId,
            BundleType = EBundleType.Combo,
            Name = name.Trim(),
            Description = description?.Trim(),
            CoverImageUrl = coverImageUrl,
            RequiredQuantity = requiredQuantity,
            DiscountType = discountType,
            DiscountValue = discountValue,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true
        };
    }

    /// <summary>
    /// Adds a book to the bundle.
    /// </summary>
    public BookBundleItem AddBook(long bookId, long? variantId = null)
    {
        if (_items.Any(i => i.BookId == bookId && i.VariantId == variantId))
            throw new InvalidOperationException("This book is already in the bundle.");

        var item = BookBundleItem.Create(Id, bookId, variantId);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;

        return item;
    }

    /// <summary>
    /// Removes a book from the bundle.
    /// </summary>
    public void RemoveBook(long bookId, long? variantId = null)
    {
        var item = _items.FirstOrDefault(i => i.BookId == bookId && i.VariantId == variantId);
        if (item != null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Updates bundle information.
    /// </summary>
    public void Update(
        string name,
        string? description,
        string? coverImageUrl,
        Money? price = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        CoverImageUrl = coverImageUrl;

        if (BundleType == EBundleType.BundleSet && price != null)
        {
            Price = price;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates combo discount settings.
    /// </summary>
    public void UpdateComboDiscount(
        int requiredQuantity,
        EDiscountType discountType,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate)
    {
        if (BundleType != EBundleType.Combo)
            throw new InvalidOperationException("Only Combo bundles can update discount settings.");

        if (requiredQuantity < 1)
            throw new ArgumentException("Required quantity must be at least 1.", nameof(requiredQuantity));

        if (discountValue <= 0)
            throw new ArgumentException("Discount value must be positive.", nameof(discountValue));

        if (discountType == EDiscountType.Percent && discountValue > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%.", nameof(discountValue));

        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));

        RequiredQuantity = requiredQuantity;
        DiscountType = discountType;
        DiscountValue = discountValue;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the bundle.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the bundle.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the bundle is valid for use.
    /// </summary>
    public bool IsValid()
    {
        if (!IsActive)
            return false;

        // BundleSet needs at least 2 items
        if (BundleType == EBundleType.BundleSet && _items.Count < 2)
            return false;

        // Combo needs to be within date range
        if (BundleType == EBundleType.Combo)
        {
            var now = DateTime.UtcNow;
            if (StartDate.HasValue && now < StartDate.Value)
                return false;
            if (EndDate.HasValue && now > EndDate.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the number of books in the bundle.
    /// </summary>
    public int BookCount => _items.Count;

    /// <summary>
    /// Calculates the discount for a Combo based on total price.
    /// </summary>
    public Money CalculateComboDiscount(Money totalPrice)
    {
        if (BundleType != EBundleType.Combo || !DiscountType.HasValue || !DiscountValue.HasValue)
            return Money.Zero();

        if (DiscountType.Value == EDiscountType.Percent)
        {
            return totalPrice.Percentage(DiscountValue.Value);
        }
        else
        {
            var discount = Money.Create(DiscountValue.Value);
            return discount > totalPrice ? totalPrice : discount;
        }
    }
}
