using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserAddress entity.
/// </summary>
public class UserAddressRepository : IUserAddressRepository
{
    private readonly WriteDbContext _context;

    public UserAddressRepository(WriteDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<UserAddress?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserAddress?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetAddressCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAddresses
            .CountAsync(a => a.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserAddress entity, CancellationToken cancellationToken = default)
    {
        await _context.UserAddresses.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(UserAddress entity)
    {
        _context.UserAddresses.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(UserAddress entity)
    {
        _context.UserAddresses.Remove(entity);
    }
}

