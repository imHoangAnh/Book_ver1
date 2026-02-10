using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using BookStation.Domain.Entities.UserAggregate;
using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Handler for CreateAddressCommand.
/// </summary>
public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, CreateAddressResponse>
{
    private readonly IUserAddressRepository _addressRepository;

    public CreateAddressCommandHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<CreateAddressResponse> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        // Create Address value object
        var address = Address.Create(
            request.Street,
            request.City,
            request.Country,
            request.Ward,
            request.District,
            request.PostalCode
        );

        // If this is set as default, remove default from other addresses
        if (request.IsDefault)
        {
            var existingAddresses = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            foreach (var existingAddress in existingAddresses.Where(a => a.IsDefault))
            {
                existingAddress.RemoveDefault();
            }
        }

        // Create UserAddress entity
        var userAddress = UserAddress.Create(
            request.UserId,
            address,
            request.Label,
            request.IsDefault,
            request.RecipientName,
            request.RecipientPhone
        );

        await _addressRepository.AddAsync(userAddress, cancellationToken);

        return new CreateAddressResponse(
            userAddress.Id,
            userAddress.Label,
            userAddress.Address.FullAddress,
            userAddress.IsDefault,
            "Address created successfully."
        );
    }
}

/// <summary>
/// Handler for UpdateAddressCommand.
/// </summary>
public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, UpdateAddressResponse>
{
    private readonly IUserAddressRepository _addressRepository;

    public UpdateAddressCommandHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<UpdateAddressResponse> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var userAddress = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);
        
        if (userAddress == null)
            throw new InvalidOperationException("Address not found.");

        if (userAddress.UserId != request.UserId)
            throw new InvalidOperationException("You do not have permission to update this address.");

        // Create new Address value object
        var address = Address.Create(
            request.Street,
            request.City,
            request.Country,
            request.Ward,
            request.District,
            request.PostalCode
        );

        userAddress.Update(address, request.Label, request.RecipientName, request.RecipientPhone);

        return new UpdateAddressResponse(
            userAddress.Id,
            userAddress.Label,
            userAddress.Address.FullAddress,
            "Address updated successfully."
        );
    }
}

/// <summary>
/// Handler for DeleteAddressCommand.
/// </summary>
public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, DeleteAddressResponse>
{
    private readonly IUserAddressRepository _addressRepository;

    public DeleteAddressCommandHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<DeleteAddressResponse> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var userAddress = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);
        
        if (userAddress == null)
            throw new InvalidOperationException("Address not found.");

        if (userAddress.UserId != request.UserId)
            throw new InvalidOperationException("You do not have permission to delete this address.");

        await _addressRepository.DeleteAsync(userAddress, cancellationToken);

        return new DeleteAddressResponse(true, "Address deleted successfully.");
    }
}

/// <summary>
/// Handler for SetDefaultAddressCommand.
/// </summary>
public class SetDefaultAddressCommandHandler : IRequestHandler<SetDefaultAddressCommand, SetDefaultAddressResponse>
{
    private readonly IUserAddressRepository _addressRepository;

    public SetDefaultAddressCommandHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<SetDefaultAddressResponse> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var userAddress = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);
        
        if (userAddress == null)
            throw new InvalidOperationException("Address not found.");

        if (userAddress.UserId != request.UserId)
            throw new InvalidOperationException("You do not have permission to update this address.");

        // Remove default from all other addresses
        var allAddresses = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        foreach (var addr in allAddresses.Where(a => a.IsDefault && a.Id != request.AddressId))
        {
            addr.RemoveDefault();
        }

        userAddress.SetAsDefault();

        return new SetDefaultAddressResponse(
            userAddress.Id,
            true,
            "Default address updated successfully."
        );
    }
}

/// <summary>
/// Handler for GetAddressesQuery.
/// </summary>
public class GetAddressesQueryHandler : IRequestHandler<GetAddressesQuery, IReadOnlyList<AddressDto>>
{
    private readonly IUserAddressRepository _addressRepository;

    public GetAddressesQueryHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<IReadOnlyList<AddressDto>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return addresses.Select(a => new AddressDto(
            a.Id,
            a.Label,
            a.Address.FullAddress,
            a.Address.Street,
            a.Address.Ward,
            a.Address.District,
            a.Address.City,
            a.Address.Country,
            a.Address.PostalCode,
            a.IsDefault,
            a.RecipientName,
            a.RecipientPhone
        )).ToList();
    }
}
