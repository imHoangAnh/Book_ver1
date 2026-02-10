using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Command to login user.
/// </summary>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;

/// <summary>
/// Response for login.
/// </summary>
public record LoginResponse(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt
);
