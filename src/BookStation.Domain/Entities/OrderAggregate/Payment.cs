using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.OrderAggregate;

/// <summary>
/// Payment entity.
/// </summary>
public class Payment : Entity<long>
{
    /// <summary>
    /// Gets the order ID.
    /// </summary>
    public long OrderId { get; private set; }

    /// <summary>
    /// Gets the payment amount.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// Gets the payment method.
    /// </summary>
    public EPaymentMethod Method { get; private set; }

    /// <summary>
    /// Gets the payment status.
    /// </summary>
    public EPaymentStatus Status { get; private set; }

    /// <summary>
    /// Gets the external transaction ID.
    /// </summary>
    public string? TransactionId { get; private set; }

    /// <summary>
    /// Gets the completion date.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Gets the failure reason if any.
    /// </summary>
    public string? FailureReason { get; private set; }

    // Navigation properties
    public Order? Order { get; private set; }

    private readonly List<Refund> _refunds = [];
    public IReadOnlyList<Refund> Refunds => _refunds.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Payment() { }

    /// <summary>
    /// Creates a new payment.
    /// </summary>
    internal static Payment Create(long orderId, Money amount, EPaymentMethod method)
    {
        return new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = EPaymentStatus.Pending
        };
    }

    /// <summary>
    /// Starts processing the payment.
    /// </summary>
    public void StartProcessing()
    {
        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can start processing.");

        Status = EPaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes the payment.
    /// </summary>
    public void Complete(string transactionId)
    {
        if (Status != EPaymentStatus.Processing && Status != EPaymentStatus.Pending)
            throw new InvalidOperationException("Only pending or processing payments can be completed.");

        Status = EPaymentStatus.Completed;
        TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Fails the payment.
    /// </summary>
    public void Fail(string reason)
    {
        if (Status == EPaymentStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed payment.");

        Status = EPaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the payment.
    /// </summary>
    public void Cancel()
    {
        if (Status == EPaymentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed payment.");

        Status = EPaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a refund for this payment.
    /// </summary>
    public Refund CreateRefund(Money amount, string reason)
    {
        if (Status != EPaymentStatus.Completed)
            throw new InvalidOperationException("Can only refund completed payments.");

        var totalRefunded = _refunds.Sum(r => r.Amount.Amount);
        if (totalRefunded + amount.Amount > Amount.Amount)
            throw new InvalidOperationException("Refund amount exceeds payment amount.");

        var refund = Refund.Create(Id, amount, reason);
        _refunds.Add(refund);
        UpdatedAt = DateTime.UtcNow;

        if (totalRefunded + amount.Amount >= Amount.Amount)
        {
            Status = EPaymentStatus.Refunded;
        }

        return refund;
    }

    /// <summary>
    /// Gets the total refunded amount.
    /// </summary>
    public Money TotalRefunded => _refunds.Aggregate(Money.Zero(), (sum, r) => sum + r.Amount);

    /// <summary>
    /// Gets a value indicating whether the payment can be refunded.
    /// </summary>
    public bool CanBeRefunded => Status == EPaymentStatus.Completed && TotalRefunded < Amount;
}
