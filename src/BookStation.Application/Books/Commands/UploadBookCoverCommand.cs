using MediatR;

namespace BookStation.Application.Books.Commands;

/// <summary>
/// Command to upload a book cover image.
/// </summary>
public record UploadBookCoverCommand(
    Stream FileStream,
    string FileName,
    long? BookId = null
) : IRequest<string>;
