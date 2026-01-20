using MediatR;

namespace BookStation.Application.Books.Commands;

/// <summary>
/// Command to create a new book.
/// </summary>
public record CreateBookCommand(
    string Title,
    string? ISBN,
    string? Description,
    string? Language,
    int? PublishYear,
    long? PublisherId,
    string? CoverImageUrl,
    int? PageCount,
    List<long>? AuthorIds,
    List<int>? CategoryIds
) : IRequest<CreateBookResponse>;

/// <summary>
/// Response for book creation.
/// </summary>
public record CreateBookResponse(
    long BookId,
    string Title,
    string? ISBN
);
