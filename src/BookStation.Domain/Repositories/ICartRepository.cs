using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.CartAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Cart aggregate.
/// </summary>
public interface ICartRepository : IRepository<Cart, long>
{
    /// <summary>
    /// Gets the active cart for a user.
    /// </summary>
    Task<Cart?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cart with its items.
    /// </summary>
    Task<Cart?> GetWithItemsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a cart for a user.
    /// </summary>
    Task<Cart> GetOrCreateAsync(long userId, CancellationToken cancellationToken = default);
}
