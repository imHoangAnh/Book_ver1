using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Handler for BecomeAuthorCommand.
/// </summary>
public class BecomeAuthorCommandHandler : IRequestHandler<BecomeAuthorCommand, BecomeAuthorResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthorRepository _authorRepository;

    public BecomeAuthorCommandHandler(
        IUserRepository userRepository,
        IAuthorRepository authorRepository)
    {
        _userRepository = userRepository;
        _authorRepository = authorRepository;
    }

    public async Task<BecomeAuthorResponse> Handle(
        BecomeAuthorCommand request,
        CancellationToken cancellationToken)
    {
        // Get the user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {request.UserId} not found.");

        // Check if author exists in catalog
        var author = await _authorRepository.GetByIdAsync(request.AuthorId, cancellationToken);
        if (author == null)
            throw new KeyNotFoundException($"Author with ID {request.AuthorId} not found.");

        // Check if user already has an author profile
        if (user.AuthorProfile != null)
        {
            throw new InvalidOperationException(
                $"User is already linked to Author ID {user.AuthorProfile.AuthorId}. " +
                "Please contact admin to update your author profile.");
        }

        // Create author profile
        var authorProfile = user.BecomeAnAuthor(request.AuthorId);
        _userRepository.Update(user);

        return new BecomeAuthorResponse(
            user.Id,
            request.AuthorId,
            authorProfile.IsVerified,
            $"Successfully linked to author '{author.FullName}'. Your profile requires admin verification to receive the blue tick."
        );
    }
}

/// <summary>
/// Handler for VerifyAuthorProfileCommand.
/// </summary>
public class VerifyAuthorProfileCommandHandler : IRequestHandler<VerifyAuthorProfileCommand, VerifyAuthorProfileResponse>
{
    private readonly IUserRepository _userRepository;

    public VerifyAuthorProfileCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<VerifyAuthorProfileResponse> Handle(
        VerifyAuthorProfileCommand request,
        CancellationToken cancellationToken)
    {
        // Get the user with author profile
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {request.UserId} not found.");

        if (user.AuthorProfile == null)
        {
            throw new InvalidOperationException(
                "User does not have an author profile. They must claim an author identity first.");
        }

        // Update verification status
        if (request.IsVerified)
        {
            user.AuthorProfile.Verify();
        }
        else
        {
            user.AuthorProfile.RevokeVerification();
        }

        _userRepository.Update(user);

        var message = request.IsVerified
            ? "Author profile verified successfully. Blue tick granted."
            : "Author profile verification revoked.";

        return new VerifyAuthorProfileResponse(
            user.Id,
            user.AuthorProfile.AuthorId,
            user.AuthorProfile.IsVerified,
            user.AuthorProfile.VerifiedAt,
            message
        );
    }
}
