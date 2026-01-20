using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;

namespace BookStation.Domain.Entities.OrderAggregate;

/// <summary>
/// Base event for order-related domain events.
/// </summary>
public abstract class OrderBaseEvent : DomainEvent
{
    public long OrderId { get; }

    protected OrderBaseEvent(long orderId)
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Event raised when a new order is created.
/// </summary>
public sealed class OrderCreatedEvent : OrderBaseEvent
{
    public long UserId { get; }
    public string ShippingAddress { get; }

    public OrderCreatedEvent(Order order) : base(order.Id)
    {
        UserId = order.UserId;
        ShippingAddress = order.ShippingAddress.FullAddress;
    }
}

/// <summary>
/// Event raised when a voucher is applied to an order.
/// </summary>
public sealed class OrderVoucherAppliedEvent : OrderBaseEvent
{
    public long VoucherId { get; }
    public decimal DiscountAmount { get; }

    public OrderVoucherAppliedEvent(long orderId, long voucherId, decimal discountAmount) : base(orderId)
    {
        VoucherId = voucherId;
        DiscountAmount = discountAmount;
    }
}

/// <summary>
/// Event raised when an order is confirmed.
/// </summary>
public sealed class OrderConfirmedEvent : OrderBaseEvent
{
    public OrderConfirmedEvent(long orderId) : base(orderId)
    {
    }
}

/// <summary>
/// Event raised when an order status changes.
/// </summary>
public sealed class OrderStatusChangedEvent : OrderBaseEvent
{
    public EOrderStatus OldStatus { get; }
    public EOrderStatus NewStatus { get; }

    public OrderStatusChangedEvent(long orderId, EOrderStatus oldStatus, EOrderStatus newStatus)
        : base(orderId)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

/// <summary>
/// Event raised when an order is shipped.
/// </summary>
public sealed class OrderShippedEvent : OrderBaseEvent
{
    public OrderShippedEvent(long orderId) : base(orderId)
    {
    }
}

/// <summary>
/// Event raised when an order is delivered.
/// </summary>
public sealed class OrderDeliveredEvent : OrderBaseEvent
{
    public OrderDeliveredEvent(long orderId) : base(orderId)
    {
    }
}

/// <summary>
/// Event raised when an order is cancelled.
/// </summary>
public sealed class OrderCancelledEvent : OrderBaseEvent
{
    public string Reason { get; }

    public OrderCancelledEvent(long orderId, string reason) : base(orderId)
    {
        Reason = reason;
    }
}

/// <summary>
/// Event raised when payment is completed.
/// </summary>
public sealed class OrderPaymentCompletedEvent : OrderBaseEvent
{
    public long PaymentId { get; }
    public decimal Amount { get; }

    public OrderPaymentCompletedEvent(long orderId, long paymentId, decimal amount) : base(orderId)
    {
        PaymentId = paymentId;
        Amount = amount;
    }
}
