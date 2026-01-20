using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.UserAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate.
/// </summary>
public interface IUserRepository : IWriteOnlyRepository<User, long>
{
    /// <summary>
    /// Gets a user by email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already in use.
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user with their roles.
    /// </summary>
    Task<User?> GetWithRolesAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user with their seller profile.
    /// </summary>
    Task<User?> GetWithSellerProfileAsync(long id, CancellationToken cancellationToken = default);
}
