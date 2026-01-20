using BookStation.Core.SharedKernel;

namespace BookStation.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address.
/// </summary>
public sealed class Address : ValueObject
{
    /// <summary>
    /// Gets the street address.
    /// </summary>
    public string Street { get; }

    /// <summary>
    /// Gets the ward/commune.
    /// </summary>
    public string? Ward { get; }

    /// <summary>
    /// Gets the district.
    /// </summary>
    public string? District { get; }

    /// <summary>
    /// Gets the city/province.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Gets the postal code.
    /// </summary>
    public string? PostalCode { get; }

    private Address(string street, string? ward, string? district, string city, string country, string? postalCode)
    {
        Street = street;
        Ward = ward;
        District = district;
        City = city;
        Country = country;
        PostalCode = postalCode;
    }

    /// <summary>
    /// Creates a new Address value object.
    /// </summary>
    public static Address Create(
        string street,
        string city,
        string country,
        string? ward = null,
        string? district = null,
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        return new Address(
            street.Trim(),
            ward?.Trim(),
            district?.Trim(),
            city.Trim(),
            country.Trim(),
            postalCode?.Trim()
        );
    }

    /// <summary>
    /// Gets the full formatted address.
    /// </summary>
    public string FullAddress
    {
        get
        {
            var parts = new List<string> { Street };

            if (!string.IsNullOrWhiteSpace(Ward))
                parts.Add(Ward);

            if (!string.IsNullOrWhiteSpace(District))
                parts.Add(District);

            parts.Add(City);

            if (!string.IsNullOrWhiteSpace(PostalCode))
                parts.Add(PostalCode);

            parts.Add(Country);

            return string.Join(", ", parts);
        }
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Street;
        yield return Ward;
        yield return District;
        yield return City;
        yield return Country;
        yield return PostalCode;
    }

    public override string ToString() => FullAddress;
}
