using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly WriteDbContext _context;

    public AuthorRepository(WriteDbContext context)
    {
        _context = context;
    }

    public async Task<Author?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Author?> GetByNameAsync(string fullName, CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .FirstOrDefaultAsync(a => a.FullName == fullName, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string fullName, CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .AnyAsync(a => a.FullName == fullName, cancellationToken);
    }

    public async Task<Author?> GetWithBooksAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Author> Authors, int TotalCount)> GetPaginatedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => 
                a.FullName.Contains(searchTerm) || 
                (a.Country != null && a.Country.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var authors = await query
            .OrderBy(a => a.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (authors, totalCount);
    }

    public async Task AddAsync(Author entity, CancellationToken cancellationToken = default)
    {
        await _context.Authors.AddAsync(entity, cancellationToken);
    }

    public void Update(Author entity)
    {
        _context.Authors.Update(entity);
    }

    public void Delete(Author entity)
    {
        _context.Authors.Remove(entity);
    }
}
