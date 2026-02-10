using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.ReviewAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Review aggregate.
/// </summary>
public interface IReviewRepository : IRepository<Review, long>
{
    Task<Review?> GetByUserAndBookAsync(long userId, long bookId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetByBookIdAsync(long bookId, CancellationToken cancellationToken = default);
}
