using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.ReviewAggregate;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly WriteDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public ReviewRepository(WriteDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task AddAsync(Review entity, CancellationToken cancellationToken = default)
    {
        await _context.Reviews.AddAsync(entity, cancellationToken);
    }

    public void Update(Review entity)
    {
        _context.Reviews.Update(entity);
    }

    public void Delete(Review entity)
    {
        _context.Reviews.Remove(entity);
    }

    public async Task<Review?> GetByUserAndBookAsync(long userId, long bookId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId, cancellationToken);
    }

    public async Task<IEnumerable<Review>> GetByBookIdAsync(long bookId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(r => r.BookId == bookId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
