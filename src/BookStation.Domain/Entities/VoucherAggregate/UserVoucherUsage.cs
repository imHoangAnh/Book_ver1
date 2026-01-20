using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.VoucherAggregate;

/// <summary>
/// User voucher usage entity - tracks when a user uses a voucher.
/// </summary>
public class UserVoucherUsage : Entity<long>
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Gets the voucher ID.
    /// </summary>
    public long VoucherId { get; private set; }

    /// <summary>
    /// Gets the order ID.
    /// </summary>
    public long OrderId { get; private set; }

    /// <summary>
    /// Gets when the voucher was used.
    /// </summary>
    public DateTime UsedAt { get; private set; }

    // Navigation properties
    public Voucher? Voucher { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private UserVoucherUsage() { }

    /// <summary>
    /// Creates a new voucher usage record.
    /// </summary>
    internal static UserVoucherUsage Create(long userId, long voucherId, long orderId)
    {
        return new UserVoucherUsage
        {
            UserId = userId,
            VoucherId = voucherId,
            OrderId = orderId,
            UsedAt = DateTime.UtcNow
        };
    }
}
