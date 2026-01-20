namespace BookStation.Core.SharedKernel;

/// <summary>
/// Base class for all entities.
/// Entities are compared by their identity (ID), not their attributes.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : IEntity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id.Equals(default!) || other.Id.Equals(default!))
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Returns a hash code for the entity.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode() * 41;
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
