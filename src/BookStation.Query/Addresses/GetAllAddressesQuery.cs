using MediatR;

namespace BookStation.Query.Addresses;

/// <summary>
/// Query to get all addresses for a specific user.
/// </summary>
public record GetAllAddressesQuery(Guid UserId) : IRequest<List<AddressDto>>;

/// <summary>
/// Address DTO.
/// </summary>
public record AddressDto(
    int Id,
    string RecipientName,
    string PhoneNumber,
    string Street,
    string? Ward,
    string City,
    string Country,
    string? PostalCode,
    string Label,
    bool IsDefault
);
