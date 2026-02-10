using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Command to register a new user.
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName,
    string? Phone = null
) : IRequest<RegisterUserResponse>;

/// <summary>
/// Response for user registration.
/// </summary>
public record RegisterUserResponse(
    Guid UserId,
    string Email,
    bool IsVerified
);
