using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.OrderAggregate;

/// <summary>
/// Order entity - Aggregate Root for order management.
/// </summary>
public class Order : AggregateRoot<long>
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Gets the total amount before discount.
    /// </summary>
    public Money TotalAmount { get; private set; } = Money.Zero();

    /// <summary>
    /// Gets the discount amount.
    /// </summary>
    public Money DiscountAmount { get; private set; } = Money.Zero();

    /// <summary>
    /// Gets the final amount after discount.
    /// </summary>
    public Money FinalAmount { get; private set; } = Money.Zero();

    /// <summary>
    /// Gets the order status.
    /// </summary>
    public EOrderStatus Status { get; private set; }

    /// <summary>
    /// Gets the voucher ID if applied.
    /// </summary>
    public long? VoucherId { get; private set; }

    /// <summary>
    /// Gets the shipping address.
    /// </summary>
    public Address ShippingAddress { get; private set; } = null!;

    /// <summary>
    /// Gets any notes for the order.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets the order confirmation date.
    /// </summary>
    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>
    /// Gets the order completion date.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Gets the cancellation date.
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// Gets the cancellation reason.
    /// </summary>
    public string? CancellationReason { get; private set; }

    // Navigation properties
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Order() { }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    public static Order Create(long userId, Address shippingAddress, string? notes = null)
    {
        var order = new Order
        {
            UserId = userId,
            ShippingAddress = shippingAddress,
            Notes = notes,
            Status = EOrderStatus.Pending,
            TotalAmount = Money.Zero(),
            DiscountAmount = Money.Zero(),
            FinalAmount = Money.Zero()
        };

        order.AddDomainEvent(new OrderCreatedEvent(order));

        return order;
    }

    /// <summary>
    /// Adds an item to the order.
    /// </summary>
    public OrderItem AddItem(long variantId, int quantity, Money unitPrice, string bookTitle, string variantName)
    {
        if (Status != EOrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order.");

        var existingItem = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = OrderItem.Create(Id, variantId, quantity, unitPrice, bookTitle, variantName);
            _items.Add(item);
        }

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;

        return _items.First(i => i.VariantId == variantId);
    }

    /// <summary>
    /// Removes an item from the order.
    /// </summary>
    public void RemoveItem(long variantId)
    {
        if (Status != EOrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from a non-pending order.");

        var item = _items.FirstOrDefault(i => i.VariantId == variantId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Applies a voucher to the order.
    /// </summary>
    public void ApplyVoucher(long voucherId, Money discountAmount)
    {
        if (Status != EOrderStatus.Pending)
            throw new InvalidOperationException("Cannot apply voucher to a non-pending order.");

        VoucherId = voucherId;
        DiscountAmount = discountAmount;
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderVoucherAppliedEvent(Id, voucherId, discountAmount.Amount));
    }

    /// <summary>
    /// Removes the applied voucher.
    /// </summary>
    public void RemoveVoucher()
    {
        if (VoucherId == null)
            return;

        VoucherId = null;
        DiscountAmount = Money.Zero();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Confirms the order.
    /// </summary>
    public void Confirm()
    {
        if (Status != EOrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed.");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an order without items.");

        Status = EOrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderConfirmedEvent(Id));
    }

    /// <summary>
    /// Starts processing the order.
    /// </summary>
    public void StartProcessing()
    {
        if (Status != EOrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can start processing.");

        Status = EOrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedEvent(Id, EOrderStatus.Confirmed, EOrderStatus.Processing));
    }

    /// <summary>
    /// Marks the order as shipped.
    /// </summary>
    public void Ship()
    {
        if (Status != EOrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped.");

        Status = EOrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderShippedEvent(Id));
    }

    /// <summary>
    /// Marks the order as delivered.
    /// </summary>
    public void Deliver()
    {
        if (Status != EOrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered.");

        Status = EOrderStatus.Delivered;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderDeliveredEvent(Id));
    }

    /// <summary>
    /// Cancels the order.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == EOrderStatus.Delivered || Status == EOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel delivered or already cancelled orders.");

        var oldStatus = Status;
        Status = EOrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    /// <summary>
    /// Adds a payment to the order.
    /// </summary>
    public Payment AddPayment(Money amount, EPaymentMethod method)
    {
        var payment = Payment.Create(Id, amount, method);
        _payments.Add(payment);
        UpdatedAt = DateTime.UtcNow;

        return payment;
    }

    /// <summary>
    /// Gets the total paid amount.
    /// </summary>
    public Money TotalPaid => _payments
        .Where(p => p.Status == EPaymentStatus.Completed)
        .Aggregate(Money.Zero(), (sum, p) => sum + p.Amount);

    /// <summary>
    /// Gets a value indicating whether the order is fully paid.
    /// </summary>
    public bool IsFullyPaid => TotalPaid >= FinalAmount;

    /// <summary>
    /// Gets a value indicating whether the order can be cancelled.
    /// </summary>
    public bool CanBeCancelled => Status is EOrderStatus.Pending or EOrderStatus.Confirmed or EOrderStatus.Processing;

    /// <summary>
    /// Gets the item count.
    /// </summary>
    public int ItemCount => _items.Sum(i => i.Quantity);

    private void RecalculateTotal()
    {
        TotalAmount = _items.Aggregate(Money.Zero(), (sum, item) => sum + item.Subtotal);
        FinalAmount = TotalAmount - DiscountAmount;
        if (FinalAmount.Amount < 0)
        {
            FinalAmount = Money.Zero();
        }
    }
}
