using MediatR;
using BookStation.Domain.ValueObjects;

namespace BookStation.Application.Books.Commands;

/// <summary>
/// Command to add a variant to a book.
/// </summary>
public record AddBookVariantCommand(
    long BookId,
    string VariantName,
    decimal Price,
    decimal? OriginalPrice = null,
    string? SKU = null,
    string? Currency = "VND",
    Guid? SellerId = null
) : IRequest<long>;

/// <summary>
/// Command to publish a book.
/// </summary>
public record PublishBookCommand(
    long BookId,
    Guid? SellerId = null
) : IRequest<bool>;
