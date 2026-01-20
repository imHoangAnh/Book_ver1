using MediatR;

namespace BookStation.Query.Books;

/// <summary>
/// Query to get book details by ID.
/// </summary>
public record GetBookByIdQuery(long BookId) : IRequest<BookDetailDto?>;

/// <summary>
/// Book detail DTO.
/// </summary>
public record BookDetailDto(
    long Id,
    string Title,
    string? ISBN,
    string? Description,
    string? Language,
    int? PublishYear,
    string? CoverImageUrl,
    int? PageCount,
    string Status,
    PublisherDto? Publisher,
    List<AuthorDto> Authors,
    List<CategoryDto> Categories,
    List<BookVariantDto> Variants,
    decimal? MinPrice,
    decimal? MaxPrice,
    DateTime CreatedAt
);

public record PublisherDto(long Id, string Name);
public record AuthorDto(long Id, string FullName, string Role);
public record CategoryDto(int Id, string Name);
public record BookVariantDto(
    long Id,
    string VariantName,
    decimal Price,
    decimal? OriginalPrice,
    bool IsAvailable,
    int? Stock
);
