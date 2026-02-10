using MediatR;

namespace BookStation.Application.Users.Commands;

// =====================================================
// AUTHOR PROFILE COMMANDS
// =====================================================

/// <summary>
/// Command for a user to claim they are an author from the catalog.
/// Creates an AuthorProfile linking the user to a catalog author.
/// </summary>
public record BecomeAuthorCommand(
    Guid UserId,
    long AuthorId
) : IRequest<BecomeAuthorResponse>;

/// <summary>
/// Response for BecomeAuthor command.
/// </summary>
public record BecomeAuthorResponse(
    Guid UserId,
    long AuthorId,
    bool IsVerified,
    string Message
);

/// <summary>
/// Command to verify or revoke verification of a user's author profile (blue tick).
/// Admin only.
/// </summary>
public record VerifyAuthorProfileCommand(
    Guid UserId,
    bool IsVerified
) : IRequest<VerifyAuthorProfileResponse>;

/// <summary>
/// Response for VerifyAuthorProfile command.
/// </summary>
public record VerifyAuthorProfileResponse(
    Guid UserId,
    long AuthorId,
    bool IsVerified,
    DateTime? VerifiedAt,
    string Message
);
