using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.VoucherAggregate;

/// <summary>
/// Voucher entity - Aggregate Root for voucher management.
/// </summary>
public class Voucher : AggregateRoot<long>
{
    /// <summary>
    /// Gets the organization ID (voucher belongs to a specific shop).
    /// </summary>
    public int? OrganizationId { get; private set; }

    /// <summary>
    /// Gets the voucher code.
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Gets the voucher name/description.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the discount type.
    /// </summary>
    public EDiscountType DiscountType { get; private set; }

    /// <summary>
    /// Gets the discount value (percentage or fixed amount).
    /// </summary>
    public decimal DiscountValue { get; private set; }

    /// <summary>
    /// Gets the minimum order amount required.
    /// </summary>
    public Money MinOrderAmount { get; private set; } = Money.Zero();

    /// <summary>
    /// Gets the maximum discount amount (for percentage discounts).
    /// </summary>
    public Money? MaxDiscountAmount { get; private set; }

    /// <summary>
    /// Gets the total usage limit (0 = unlimited).
    /// </summary>
    public int UsageLimit { get; private set; }

    /// <summary>
    /// Gets the current usage count.
    /// </summary>
    public int UsageCount { get; private set; }

    /// <summary>
    /// Gets the start date.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the voucher is active.
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<UserVoucherUsage> _usages = [];
    public IReadOnlyList<UserVoucherUsage> Usages => _usages.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Voucher() { }

    /// <summary>
    /// Creates a new voucher.
    /// </summary>
    public static Voucher Create(
        string code,
        string name,
        EDiscountType discountType,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate,
        Money? minOrderAmount = null,
        Money? maxDiscountAmount = null,
        int usageLimit = 0,
        int? organizationId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (discountValue <= 0)
            throw new ArgumentException("Discount value must be positive.", nameof(discountValue));

        if (discountType == EDiscountType.Percent && discountValue > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%.", nameof(discountValue));

        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));

        return new Voucher
        {
            Code = code.ToUpperInvariant().Trim(),
            Name = name.Trim(),
            DiscountType = discountType,
            DiscountValue = discountValue,
            StartDate = startDate,
            EndDate = endDate,
            MinOrderAmount = minOrderAmount ?? Money.Zero(),
            MaxDiscountAmount = maxDiscountAmount,
            UsageLimit = usageLimit,
            UsageCount = 0,
            IsActive = true,
            OrganizationId = organizationId
        };
    }

    /// <summary>
    /// Updates the voucher.
    /// </summary>
    public void Update(
        string name,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate,
        Money? minOrderAmount,
        Money? maxDiscountAmount,
        int usageLimit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        DiscountValue = discountValue;
        StartDate = startDate;
        EndDate = endDate;
        MinOrderAmount = minOrderAmount ?? Money.Zero();
        MaxDiscountAmount = maxDiscountAmount;
        UsageLimit = usageLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the voucher is valid for use.
    /// </summary>
    public bool IsValid()
    {
        if (!IsActive)
            return false;

        var now = DateTime.UtcNow;
        if (now < StartDate || now > EndDate)
            return false;

        if (UsageLimit > 0 && UsageCount >= UsageLimit)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if the voucher can be used for the given order amount.
    /// </summary>
    public bool CanApply(Money orderAmount)
    {
        if (!IsValid())
            return false;

        if (orderAmount < MinOrderAmount)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a user has already used this voucher.
    /// </summary>
    public bool HasUserUsed(long userId)
    {
        return _usages.Any(u => u.UserId == userId);
    }

    /// <summary>
    /// Calculates the discount amount for the given order total.
    /// </summary>
    public Money CalculateDiscount(Money orderTotal)
    {
        if (!CanApply(orderTotal))
            return Money.Zero();

        Money discount;
        if (DiscountType == EDiscountType.Percent)
        {
            discount = orderTotal.Percentage(DiscountValue);
            if (MaxDiscountAmount != null && discount > MaxDiscountAmount)
            {
                discount = MaxDiscountAmount;
            }
        }
        else
        {
            discount = Money.Create(DiscountValue);
            if (discount > orderTotal)
            {
                discount = orderTotal;
            }
        }

        return discount;
    }

    /// <summary>
    /// Records a usage of this voucher.
    /// </summary>
    public UserVoucherUsage Use(long userId, long orderId)
    {
        if (!IsValid())
            throw new InvalidOperationException("Voucher is not valid.");

        if (HasUserUsed(userId))
            throw new InvalidOperationException("User has already used this voucher.");

        var usage = UserVoucherUsage.Create(userId, Id, orderId);
        _usages.Add(usage);
        UsageCount++;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new VoucherUsedEvent(Id, userId, orderId));

        return usage;
    }

    /// <summary>
    /// Activates the voucher.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the voucher.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets remaining uses (0 if unlimited).
    /// </summary>
    public int RemainingUses => UsageLimit > 0 ? Math.Max(0, UsageLimit - UsageCount) : int.MaxValue;
}
