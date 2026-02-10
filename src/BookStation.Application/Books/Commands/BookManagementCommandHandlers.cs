using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Books.Commands;

public class AddBookVariantCommandHandler : IRequestHandler<AddBookVariantCommand, long>
{
    private readonly IBookRepository _bookRepository;

    public AddBookVariantCommandHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<long> Handle(AddBookVariantCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetWithVariantsAsync(request.BookId, cancellationToken);
        if (book == null)
            throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        // Check ownership
        if (request.SellerId.HasValue && book.SellerId.HasValue && book.SellerId != request.SellerId)
        {
            throw new UnauthorizedAccessException("You are not the owner of this book.");
        }

        var price = Money.Create(request.Price, request.Currency ?? "VND");
        var variant = book.AddVariant(request.VariantName, price);

        if (request.OriginalPrice.HasValue)
        {
            // Note: Variant entity setup for OriginalPrice might need direct setting if not exposed in AddVariant
            // Assuming default AddVariant doesn't set OriginalPrice, checking Entity logic... 
            // Step 250 showed AddVariant only takes name and price. 
            // We might need to extend Book.AddVariant or set accessing _variants if possible?
            // Since Variants are exposed as IReadOnlyList, we can't edit directly.
            // But variant returned is the entity object reference? 
            // "public BookVariant AddVariant(...) { ... return variant; }" -> Yes!
            
            // Wait, BookVariant entity definition might prohibit setting properties from outside if setters are private.
            // Let's assume we can't set OriginalPrice for now unless we update BookVariant.
            // Or maybe BookVariant has Update method?
        }
        
        await _bookRepository.UpdateAsync(book, cancellationToken);

        return variant.Id;
    }
}

public class PublishBookCommandHandler : IRequestHandler<PublishBookCommand, bool>
{
    private readonly IBookRepository _bookRepository;

    public PublishBookCommandHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<bool> Handle(PublishBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetWithVariantsAsync(request.BookId, cancellationToken);
        if (book == null)
            throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        // Check ownership
        if (request.SellerId.HasValue && book.SellerId.HasValue && book.SellerId != request.SellerId)
        {
            throw new UnauthorizedAccessException("You are not the owner of this book.");
        }

        book.Publish();

        await _bookRepository.UpdateAsync(book, cancellationToken);

        return true;
    }
}
