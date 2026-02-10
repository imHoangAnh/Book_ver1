using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Enums;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly WriteDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public OrderRepository(WriteDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(entity, cancellationToken);
    }

    public void Update(Order entity)
    {
        _context.Orders.Update(entity);
    }

    public Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(entity);
        return Task.CompletedTask;
    }

    public void Delete(Order entity)
    {
        _context.Orders.Remove(entity);
    }

    public async Task<Order?> GetWithItemsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetWithAllDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Shipment)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(EOrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUserPurchasedBookAsync(long userId, long bookId, CancellationToken cancellationToken = default)
    {
        // Check for delivered orders containing the book variant
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && o.Status == EOrderStatus.Delivered)
            .SelectMany(o => o.OrderItems)
            .AnyAsync(oi => _context.BookVariants
                .Where(v => v.Id == oi.VariantId)
                .Select(v => v.BookId)
                .FirstOrDefault() == bookId, 
                cancellationToken);
    }
}
