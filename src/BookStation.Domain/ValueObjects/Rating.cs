using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Rating value object (1-5 stars).
/// </summary>
public class Rating : ValueObject
{
    public int Value { get; }

    private Rating(int value)
    {
        Value = value;
    }

    public static Rating Create(int value)
    {
        if (value < 1 || value > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(value));

        return new Rating(value);
    }

    public static implicit operator int(Rating rating) => rating.Value;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
