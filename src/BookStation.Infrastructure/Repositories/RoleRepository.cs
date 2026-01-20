using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Role aggregate.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly WriteDbContext _context;

    public RoleRepository(WriteDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<Role?> GetWithPermissionsAsync(long roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<List<Role>> GetAllActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AnyAsync(r => r.Name == name, cancellationToken);
    }

    public async Task AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(entity, cancellationToken);
    }

    public void Update(Role entity)
    {
        _context.Roles.Update(entity);
    }

    public void Delete(Role entity)
    {
        _context.Roles.Remove(entity);
    }
}
