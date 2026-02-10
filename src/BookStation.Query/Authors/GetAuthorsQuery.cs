using BookStation.Core.Pagination;
using BookStation.Query.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Query.Authors;

/// <summary>
/// Query to get all authors with pagination.
/// </summary>
public record GetAuthorsQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PaginatedList<AuthorListDto>>;

/// <summary>
/// Author list DTO for search results.
/// </summary>
public record AuthorListDto(
    long Id,
    string FullName,
    string? Bio,
    string? Country,
    string? PhotoUrl,
    bool IsVerified,
    int BookCount
);

/// <summary>
/// Handler for GetAuthorsQuery.
/// </summary>
public class GetAuthorsQueryHandler : IRequestHandler<GetAuthorsQuery, PaginatedList<AuthorListDto>>
{
    private readonly IReadDbContext _context;

    public GetAuthorsQueryHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuthorListDto>> Handle(
        GetAuthorsQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Query<Domain.Entities.CatalogAggregate.Author>()
            .AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLower();
            query = query.Where(a => 
                a.FullName.ToLower().Contains(searchTerm) ||
                (a.Country != null && a.Country.ToLower().Contains(searchTerm)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated authors
        var authors = await query
            .OrderBy(a => a.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new { a.Id, a.FullName, a.Bio, a.Country, a.PhotoUrl, BookCount = a.BookAuthors.Count })
            .ToListAsync(cancellationToken);

        // Get verification status from AuthorProfiles
        var authorIds = authors.Select(a => a.Id).ToList();
        var verifiedAuthorIds = await _context.Query<Domain.Entities.UserAggregate.AuthorProfile>()
            .AsNoTracking()
            .Where(ap => authorIds.Contains(ap.AuthorId) && ap.IsVerified)
            .Select(ap => ap.AuthorId)
            .ToListAsync(cancellationToken);

        // Map to DTOs with verification status
        var result = authors.Select(a => new AuthorListDto(
            a.Id,
            a.FullName,
            a.Bio != null ? (a.Bio.Length > 200 ? a.Bio.Substring(0, 200) + "..." : a.Bio) : null,
            a.Country,
            a.PhotoUrl,
            verifiedAuthorIds.Contains(a.Id),
            a.BookCount
        )).ToList();

        return new PaginatedList<AuthorListDto>(
            result,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
