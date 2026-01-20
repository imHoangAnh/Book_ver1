namespace BookStation.Core.SharedKernel;

/// <summary>
/// Marker interface for entities.
/// </summary>
public interface IEntity
{
}

/// <summary>
/// Generic marker interface for entities with a specific ID type.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IEntity<TId> : IEntity
    where TId : IEquatable<TId>
{
    TId Id { get; }
}
