using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>
    /// Gets the amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency code (ISO 4217).
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Default currency for the system.
    /// </summary>
    public const string DefaultCurrency = "VND";

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a new Money value object.
    /// </summary>
    public static Money Create(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    /// <summary>
    /// Creates a zero Money value object.
    /// </summary>
    public static Money Zero(string currency = DefaultCurrency) => new(0, currency);

    /// <summary>
    /// Adds two Money values.
    /// </summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts a Money value from this one.
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        return new Money(result >= 0 ? result : 0, Currency);
    }

    /// <summary>
    /// Multiplies the amount by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative.", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Calculates a percentage of this Money value.
    /// </summary>
    public Money Percentage(decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentException("Percent must be between 0 and 100.", nameof(percent));

        return new Money(Amount * percent / 100, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot perform operation on different currencies: {Currency} and {other.Currency}");
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N0} {Currency}";

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);
    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;
}
