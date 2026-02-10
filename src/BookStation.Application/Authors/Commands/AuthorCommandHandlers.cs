using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.Repositories;
using MediatR;

namespace BookStation.Application.Authors.Commands;

/// <summary>
/// Handler for CreateAuthorCommand.
/// </summary>
public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, CreateAuthorResponse>
{
    private readonly IAuthorRepository _authorRepository;

    public CreateAuthorCommandHandler(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<CreateAuthorResponse> Handle(
        CreateAuthorCommand request,
        CancellationToken cancellationToken)
    {
        // Check if author with same name already exists
        var exists = await _authorRepository.ExistsByNameAsync(request.FullName, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Author with name '{request.FullName}' already exists.");
        }

        // Create author entity
        var author = Author.Create(
            request.FullName,
            request.Bio,
            request.DateOfBirth,
            request.Country
        );

        await _authorRepository.AddAsync(author, cancellationToken);

        return new CreateAuthorResponse(author.Id, author.FullName);
    }
}

/// <summary>
/// Handler for UpdateAuthorCommand.
/// </summary>
public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, bool>
{
    private readonly IAuthorRepository _authorRepository;

    public UpdateAuthorCommandHandler(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<bool> Handle(
        UpdateAuthorCommand request,
        CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetByIdAsync(request.AuthorId, cancellationToken);
        if (author == null)
        {
            throw new KeyNotFoundException($"Author with ID {request.AuthorId} not found.");
        }

        // Check if new name conflicts with another author
        if (author.FullName != request.FullName)
        {
            var existingAuthor = await _authorRepository.GetByNameAsync(request.FullName, cancellationToken);
            if (existingAuthor != null && existingAuthor.Id != request.AuthorId)
            {
                throw new InvalidOperationException($"Author with name '{request.FullName}' already exists.");
            }
        }

        author.Update(
            request.FullName,
            request.Bio,
            request.DateOfBirth,
            request.DiedDate,
            request.Address,
            request.Country,
            request.PhotoUrl
        );

        _authorRepository.Update(author);

        return true;
    }
}

/// <summary>
/// Handler for DeleteAuthorCommand.
/// </summary>
public class DeleteAuthorCommandHandler : IRequestHandler<DeleteAuthorCommand, bool>
{
    private readonly IAuthorRepository _authorRepository;

    public DeleteAuthorCommandHandler(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<bool> Handle(
        DeleteAuthorCommand request,
        CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetWithBooksAsync(request.AuthorId, cancellationToken);
        if (author == null)
        {
            throw new KeyNotFoundException($"Author with ID {request.AuthorId} not found.");
        }

        // Check if author has books
        if (author.BookCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete author '{author.FullName}' because they have {author.BookCount} associated book(s). " +
                "Please remove the author from all books first.");
        }

        _authorRepository.Delete(author);

        return true;
    }
}
