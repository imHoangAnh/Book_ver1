namespace BookStation.Core.SharedKernel;

/// <summary>
/// Base class for value objects.
/// Value objects are immutable and compared by their properties, not identity.
/// Examples: Email, Money, Address.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values of the value object for equality comparison.
    /// </summary>
    protected abstract IEnumerable<object?> GetAtomicValues();

    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        if (GetType() != other.GetType())
            return false;

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other)
            return false;

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(default(int), HashCode.Combine);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
