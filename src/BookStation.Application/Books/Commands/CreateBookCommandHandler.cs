using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Books.Commands;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, CreateBookResponse>
{
    private readonly IBookRepository _bookRepository;

    public CreateBookCommandHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<CreateBookResponse> Handle(
        CreateBookCommand request,
        CancellationToken cancellationToken)
    {
        // Create ISBN value object if provided
        ISBN? isbn = null;
        if (!string.IsNullOrWhiteSpace(request.ISBN))
        {
            isbn = ISBN.Create(request.ISBN);

            // Check if ISBN is unique
            var isUnique = await _bookRepository.IsISBNUniqueAsync(request.ISBN, cancellationToken);
            if (!isUnique)
            {
                throw new InvalidOperationException($"ISBN '{request.ISBN}' is already in use.");
            }
        }

        // Create book entity
        var book = Book.Create(
            request.Title,
            isbn,
            request.Description,
            request.Language,
            request.PublishYear,
            request.PublisherId,
            request.CoverImageUrl,
            request.PageCount
        );

        // Add authors
        if (request.AuthorIds?.Any() == true)
        {
            foreach (var authorId in request.AuthorIds)
            {
                book.AddAuthor(authorId);
            }
        }

        // Add categories
        if (request.CategoryIds?.Any() == true)
        {
            foreach (var categoryId in request.CategoryIds)
            {
                book.AddCategory(categoryId);
            }
        }

        // Save book
        await _bookRepository.AddAsync(book, cancellationToken);

        return new CreateBookResponse(
            book.Id,
            book.Title,
            book.ISBN?.Value
        );
    }
}
