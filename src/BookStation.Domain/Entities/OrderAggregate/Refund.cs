using BookStation.Core.SharedKernel;
using BookStation.Domain.ValueObjects;

namespace BookStation.Domain.Entities.OrderAggregate;

/// <summary>
/// Refund entity.
/// </summary>
public class Refund : Entity<long>
{
    /// <summary>
    /// Gets the payment ID.
    /// </summary>
    public long PaymentId { get; private set; }

    /// <summary>
    /// Gets the refund amount.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// Gets the refund reason.
    /// </summary>
    public string Reason { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the refund is processed.
    /// </summary>
    public bool IsProcessed { get; private set; }

    /// <summary>
    /// Gets the processing date.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    // Navigation properties
    public Payment? Payment { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Refund() { }

    /// <summary>
    /// Creates a new refund.
    /// </summary>
    internal static Refund Create(long paymentId, Money amount, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty.", nameof(reason));

        return new Refund
        {
            PaymentId = paymentId,
            Amount = amount,
            Reason = reason.Trim(),
            IsProcessed = false
        };
    }

    /// <summary>
    /// Marks the refund as processed.
    /// </summary>
    public void Process()
    {
        if (IsProcessed)
            throw new InvalidOperationException("Refund is already processed.");

        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
