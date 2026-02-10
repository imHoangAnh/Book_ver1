using BookStation.Application.Services;
using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.Repositories;
using MediatR;

namespace BookStation.Application.Books.Commands;

public class UploadBookCoverCommandHandler : IRequestHandler<UploadBookCoverCommand, string>
{
    private readonly IImageUploadService _imageUploadService;
    private readonly IBookRepository _bookRepository;

    public UploadBookCoverCommandHandler(
        IImageUploadService imageUploadService,
        IBookRepository bookRepository)
    {
        _imageUploadService = imageUploadService;
        _bookRepository = bookRepository;
    }

    public async Task<string> Handle(
        UploadBookCoverCommand request,
        CancellationToken cancellationToken)
    {
        // If updating existing book, verify it exists first
        Book? book = null;
        if (request.BookId.HasValue)
        {
            book = await _bookRepository.GetByIdAsync(request.BookId.Value, cancellationToken);
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");
            }
        }

        // Upload image to Cloudinary using the dedicated book cover upload method
        var uploadResult = await _imageUploadService.UploadBookCoverAsync(
            request.FileStream,
            request.FileName,
            cancellationToken
        );

        if (!uploadResult.Success)
        {
            throw new InvalidOperationException($"Failed to upload image: {uploadResult.Error}");
        }

        // If updating existing book, save the URL
        if (book != null)
        {
            book.UpdateCoverImage(uploadResult.Url);
            _bookRepository.Update(book);
        }

        return uploadResult.Url;
    }
}
