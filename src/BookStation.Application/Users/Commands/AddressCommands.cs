using MediatR;

namespace BookStation.Application.Users.Commands;

// =====================================================
// ADDRESS COMMANDS
// =====================================================

/// <summary>
/// Command to create a new address for a user.
/// </summary>
public record CreateAddressCommand(
    Guid UserId,
    string Street,
    string City,
    string Country,
    string? Ward = null,
    string? District = null,
    string? PostalCode = null,
    string? Label = "Home",
    bool IsDefault = false,
    string? RecipientName = null,
    string? RecipientPhone = null
) : IRequest<CreateAddressResponse>;

/// <summary>
/// Response for creating an address.
/// </summary>
public record CreateAddressResponse(
    int AddressId,
    string Label,
    string FullAddress,
    bool IsDefault,
    string Message
);

/// <summary>
/// Command to update an existing address.
/// </summary>
public record UpdateAddressCommand(
    Guid UserId,
    int AddressId,
    string Street,
    string City,
    string Country,
    string? Ward = null,
    string? District = null,
    string? PostalCode = null,
    string? Label = null,
    string? RecipientName = null,
    string? RecipientPhone = null
) : IRequest<UpdateAddressResponse>;

/// <summary>
/// Response for updating an address.
/// </summary>
public record UpdateAddressResponse(
    int AddressId,
    string Label,
    string FullAddress,
    string Message
);

/// <summary>
/// Command to delete an address.
/// </summary>
public record DeleteAddressCommand(
    Guid UserId,
    int AddressId
) : IRequest<DeleteAddressResponse>;

/// <summary>
/// Response for deleting an address.
/// </summary>
public record DeleteAddressResponse(
    bool Success,
    string Message
);

/// <summary>
/// Command to set an address as default.
/// </summary>
public record SetDefaultAddressCommand(
    Guid UserId,
    int AddressId
) : IRequest<SetDefaultAddressResponse>;

/// <summary>
/// Response for setting default address.
/// </summary>
public record SetDefaultAddressResponse(
    int AddressId,
    bool Success,
    string Message
);

/// <summary>
/// Query to get all addresses for a user.
/// </summary>
public record GetAddressesQuery(Guid UserId) : IRequest<IReadOnlyList<AddressDto>>;

/// <summary>
/// DTO for address information.
/// </summary>
public record AddressDto(
    int Id,
    string Label,
    string FullAddress,
    string Street,
    string? Ward,
    string? District,
    string City,
    string Country,
    string? PostalCode,
    bool IsDefault,
    string? RecipientName,
    string? RecipientPhone
);
