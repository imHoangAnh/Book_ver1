using MediatR;
using Microsoft.EntityFrameworkCore;
using BookStation.Query.Abstractions;

namespace BookStation.Query.Addresses;

public class GetAllAddressesQueryHandler : IRequestHandler<GetAllAddressesQuery, List<AddressDto>>
{
    private readonly IReadDbContext _dbContext;

    public GetAllAddressesQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AddressDto>> Handle(
        GetAllAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var addresses = await _dbContext.UserAddresses
            .AsNoTracking()
            .Where(ua => ua.UserId == request.UserId)
            .OrderByDescending(ua => ua.IsDefault)
            .ThenByDescending(ua => ua.CreatedAt)
            .Select(ua => new AddressDto(
                ua.Id,
                ua.RecipientName ?? string.Empty,
                ua.RecipientPhone ?? string.Empty,
                ua.Address.Street,
                ua.Address.Ward ?? string.Empty,
                ua.Address.City,
                ua.Address.Country,
                ua.Address.PostalCode,
                ua.Label,
                ua.IsDefault
            ))
            .ToListAsync(cancellationToken);

        return addresses;
    }
}
