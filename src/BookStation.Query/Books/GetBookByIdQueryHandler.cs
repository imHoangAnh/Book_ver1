using MediatR;
using Microsoft.EntityFrameworkCore;
using BookStation.Query.Abstractions;

namespace BookStation.Query.Books;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDetailDto?>
{
    private readonly IReadDbContext _dbContext;

    public GetBookByIdQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookDetailDto?> Handle(
        GetBookByIdQuery request,
        CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books
            .Where(b => b.Id == request.BookId)
            .Select(b => new BookDetailDto(
                b.Id,
                b.Title,
                b.ISBN,
                b.Description,
                b.Language,
                b.PublishYear,
                b.CoverImageUrl,
                b.PageCount,
                b.Status.ToString(),
                b.Publisher != null ? new PublisherDto(b.Publisher.Id, b.Publisher.Name) : null,
                b.BookAuthors.Select(ba => new AuthorDto(
                    ba.Author!.Id,
                    ba.Author.FullName,
                    ba.Role.ToString()
                )).ToList(),
                b.BookCategories.Select(bc => new CategoryDto(
                    bc.Category!.Id,
                    bc.Category.Name
                )).ToList(),
                b.Variants.Select(v => new BookVariantDto(
                    v.Id,
                    v.VariantName,
                    v.Price.Amount,
                    v.OriginalPrice != null ? v.OriginalPrice.Amount : null,
                    v.IsAvailable,
                    v.Inventory != null ? v.Inventory.Stock : null
                )).ToList(),
                b.Variants.Any() ? b.Variants.Min(v => v.Price.Amount) : null,
                b.Variants.Any() ? b.Variants.Max(v => v.Price.Amount) : null,
                b.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return book;
    }
}
