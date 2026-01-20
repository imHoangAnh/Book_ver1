using MediatR;

namespace BookStation.Core.SharedKernel;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that 
/// domain experts care about.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time when the event occurred in UTC.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
