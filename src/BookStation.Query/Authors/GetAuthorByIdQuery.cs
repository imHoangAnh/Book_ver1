using BookStation.Query.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Query.Authors;

/// <summary>
/// Query to get author details by ID.
/// </summary>
public record GetAuthorByIdQuery(long AuthorId) : IRequest<AuthorDetailDto?>;

/// <summary>
/// Author detail DTO.
/// </summary>
public record AuthorDetailDto(
    long Id,
    string FullName,
    string? Bio,
    DateTime? DateOfBirth,
    DateTime? DiedDate,
    string? Address,
    string? Country,
    string? PhotoUrl,
    bool IsVerified,
    DateTime? VerifiedAt,
    Guid? LinkedUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<AuthorBookDto> Books
);

/// <summary>
/// Book info for author detail.
/// </summary>
public record AuthorBookDto(
    long BookId,
    string Title,
    string? CoverImageUrl
);

/// <summary>
/// Handler for GetAuthorByIdQuery.
/// </summary>
public class GetAuthorByIdQueryHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDetailDto?>
{
    private readonly IReadDbContext _context;

    public GetAuthorByIdQueryHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<AuthorDetailDto?> Handle(
        GetAuthorByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var author = await _context.Query<Domain.Entities.CatalogAggregate.Author>()
            .AsNoTracking()
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == request.AuthorId, cancellationToken);

        if (author == null)
            return null;

        // Check if any user has claimed and verified this author
        var authorProfile = await _context.Query<Domain.Entities.UserAggregate.AuthorProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ap => ap.AuthorId == request.AuthorId, cancellationToken);

        // Get book details for this author
        var bookIds = author.BookAuthors.Select(ba => ba.BookId).ToList();
        var books = await _context.Query<Domain.Entities.CatalogAggregate.Book>()
            .AsNoTracking()
            .Where(b => bookIds.Contains(b.Id))
            .Select(b => new { b.Id, b.Title, b.CoverImageUrl })
            .ToListAsync(cancellationToken);

        var authorBooks = author.BookAuthors
            .Select(ba => new AuthorBookDto(
                ba.BookId,
                books.FirstOrDefault(b => b.Id == ba.BookId)?.Title ?? "Unknown",
                books.FirstOrDefault(b => b.Id == ba.BookId)?.CoverImageUrl
            ))
            .ToList();

        return new AuthorDetailDto(
            author.Id,
            author.FullName,
            author.Bio,
            author.DateOfBirth,
            author.DiedDate,
            author.Address,
            author.Country,
            author.PhotoUrl,
            authorProfile?.IsVerified ?? false,
            authorProfile?.VerifiedAt,
            authorProfile?.Id, // LinkedUserId is the AuthorProfile.Id which equals User.Id
            author.CreatedAt,
            author.UpdatedAt,
            authorBooks
        );
    }
}
