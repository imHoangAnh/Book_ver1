using BookStation.Domain.Entities.UserAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for UserAddress entity.
/// </summary>
public interface IUserAddressRepository
{
    /// <summary>
    /// Gets an address by its identifier.
    /// </summary>
    Task<UserAddress?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all addresses for a specific user.
    /// </summary>
    Task<IReadOnlyList<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default address for a specific user.
    /// </summary>
    Task<UserAddress?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of addresses for a specific user.
    /// </summary>
    Task<int> GetAddressCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new address.
    /// </summary>
    Task AddAsync(UserAddress entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing address.
    /// </summary>
    void Update(UserAddress entity);

    /// <summary>
    /// Deletes an address.
    /// </summary>
    void Delete(UserAddress entity);
}

