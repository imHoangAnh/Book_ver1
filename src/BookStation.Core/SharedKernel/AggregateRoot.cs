namespace BookStation.Core.SharedKernel;

/// <summary>
/// Base class for aggregate roots.
/// Aggregate roots own their domain events and control access to aggregate members.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : IEquatable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    protected AggregateRoot()
    {
    }
    
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Gets the domain events snapshot.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => [.. _domainEvents];

    /// <summary>
    /// Clears all the domain events from the aggregate.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Adds the specified domain event to the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
