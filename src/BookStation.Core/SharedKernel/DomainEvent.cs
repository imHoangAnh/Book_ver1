namespace BookStation.Core.SharedKernel;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class.
    /// </summary>
    protected DomainEvent()
    {
        OccurredOnUtc = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }

    /// <summary>
    /// Gets the unique identifier of this event.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Gets the date and time when the event occurred in UTC.
    /// </summary>
    public DateTime OccurredOnUtc { get; }
}
