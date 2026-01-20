using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WriteDbContext _context;

    public UserRepository(WriteDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return !await _context.Users
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<User?> GetWithRolesAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetWithSellerProfileAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(entity, cancellationToken);
    }

    public void Update(User entity)
    {
        _context.Users.Update(entity);
    }

    public void Delete(User entity)
    {
        _context.Users.Remove(entity);
    }
}
