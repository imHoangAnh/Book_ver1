using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Enums;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Order aggregate.
/// </summary>
public interface IOrderRepository : IWriteOnlyRepository<Order, long>
{
    /// <summary>
    /// Gets an order with its items.
    /// </summary>
    Task<Order?> GetWithItemsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order with all related data.
    /// </summary>
    Task<Order?> GetWithAllDetailsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders for a user.
    /// </summary>
    Task<IEnumerable<Order>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by status.
    /// </summary>
    Task<IEnumerable<Order>> GetByStatusAsync(EOrderStatus status, CancellationToken cancellationToken = default);
}
