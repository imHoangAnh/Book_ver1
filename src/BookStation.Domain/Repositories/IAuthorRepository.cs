using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.CatalogAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Author entity (catalog data only).
/// </summary>
public interface IAuthorRepository : IRepository<Author, long>
{
    /// <summary>
    /// Gets an author by full name.
    /// </summary>
    Task<Author?> GetByNameAsync(string fullName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an author with the given name exists.
    /// </summary>
    Task<bool> ExistsByNameAsync(string fullName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an author with their books.
    /// </summary>
    Task<Author?> GetWithBooksAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all authors with pagination.
    /// </summary>
    Task<(IEnumerable<Author> Authors, int TotalCount)> GetPaginatedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
