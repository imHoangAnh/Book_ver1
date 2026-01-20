using BookStation.Core.Pagination;
using BookStation.Query.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Query.Books;

public class SearchBooksQueryHandler : IRequestHandler<SearchBooksQuery, PaginatedList<BookListDto>>
{
    private readonly IReadDbContext _dbContext;

    public SearchBooksQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<BookListDto>> Handle(
        SearchBooksQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Books.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(searchTerm) ||
                (b.ISBN != null && b.ISBN.ToLower().Contains(searchTerm)) ||
                (b.Description != null && b.Description.ToLower().Contains(searchTerm))
            );
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == request.CategoryId.Value));
        }

        if (request.PublisherId.HasValue)
        {
            query = query.Where(b => b.PublisherId == request.PublisherId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(b => b.Variants.Any(v => v.Price.Amount >= request.MinPrice.Value));
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(b => b.Variants.Any(v => v.Price.Amount <= request.MaxPrice.Value));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "price" => request.SortDescending
                ? query.OrderByDescending(b => b.Variants.Min(v => v.Price.Amount))
                : query.OrderBy(b => b.Variants.Min(v => v.Price.Amount)),
            "date" => request.SortDescending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt),
            _ => request.SortDescending
                ? query.OrderByDescending(b => b.Title)
                : query.OrderBy(b => b.Title)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BookListDto(
                b.Id,
                b.Title,
                b.ISBN,
                b.CoverImageUrl,
                b.Variants.Any() ? b.Variants.Min(v => v.Price.Amount) : null,
                b.Variants.Any() ? b.Variants.Max(v => v.Price.Amount) : null,
                b.Status.ToString(),
                b.BookAuthors.Select(ba => ba.Author!.FullName).ToList(),
                b.BookCategories.Select(bc => bc.Category!.Name).ToList()
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<BookListDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
