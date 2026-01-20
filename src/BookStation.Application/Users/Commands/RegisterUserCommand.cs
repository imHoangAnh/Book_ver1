using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Command to register a new user.
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string? FullName = null,
    string? Phone = null
) : IRequest<RegisterUserResponse>;

/// <summary>
/// Response for user registration.
/// </summary>
public record RegisterUserResponse(
    long UserId,
    string Email,
    bool IsVerified
);
