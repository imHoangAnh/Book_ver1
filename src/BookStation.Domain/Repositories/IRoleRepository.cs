using BookStation.Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Role aggregate.
/// </summary>
public interface IRoleRepository : IWriteOnlyRepository<Role, long>
{
    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role with its permissions.
    /// </summary>
    Task<Role?> GetWithPermissionsAsync(long roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<List<Role>> GetAllActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role exists by name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
