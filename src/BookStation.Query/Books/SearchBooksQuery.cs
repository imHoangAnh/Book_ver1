using BookStation.Core.Pagination;
using MediatR;

namespace BookStation.Query.Books;

/// <summary>
/// Query to search books with pagination.
/// </summary>
public record SearchBooksQuery(
    string? SearchTerm = null,
    int? CategoryId = null,
    long? PublisherId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = "title",
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PaginatedList<BookListDto>>;

/// <summary>
/// Book list DTO for search results.
/// </summary>
public record BookListDto(
    long Id,
    string Title,
    string? ISBN,
    string? CoverImageUrl,
    decimal? MinPrice,
    decimal? MaxPrice,
    string Status,
    List<string> Authors,
    List<string> Categories
);
