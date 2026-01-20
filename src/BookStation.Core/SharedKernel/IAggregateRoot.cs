namespace BookStation.Core.SharedKernel;

/// <summary>
/// Interface for aggregate roots.
/// An Aggregate Root is the entry point to an aggregate, 
/// which is a cluster of domain objects that can be treated as a single unit.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the collection of domain events.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this aggregate.
    /// </summary>
    void ClearDomainEvents();
}
