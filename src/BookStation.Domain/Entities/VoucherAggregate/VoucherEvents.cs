using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.VoucherAggregate;

/// <summary>
/// Event raised when a voucher is used.
/// </summary>
public sealed class VoucherUsedEvent : DomainEvent
{
    public long VoucherId { get; }
    public long UserId { get; }
    public long OrderId { get; }

    public VoucherUsedEvent(long voucherId, long userId, long orderId)
    {
        VoucherId = voucherId;
        UserId = userId;
        OrderId = orderId;
    }
}
