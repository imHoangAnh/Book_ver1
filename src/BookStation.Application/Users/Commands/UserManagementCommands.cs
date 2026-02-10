using MediatR;

namespace BookStation.Application.Users.Commands;

// =====================================================
// ADMIN USER MANAGEMENT COMMANDS (for UsersController)
// =====================================================

/// <summary>
/// Command to verify a user (Pending → Active).
/// </summary>
/// <summary>
/// Command to verify a user (Pending → Active).
/// </summary>
public record VerifyUserCommand(Guid UserId) : IRequest<UserStatusResponse>;

/// <summary>
/// Command to ban a user.
/// </summary>
public record BanUserCommand(Guid UserId, string Reason) : IRequest<UserStatusResponse>;

/// <summary>
/// Command to unban a user.
/// </summary>
public record UnbanUserCommand(Guid UserId) : IRequest<UserStatusResponse>;

/// <summary>
/// Command to suspend a user temporarily.
/// </summary>
public record SuspendUserCommand(
    Guid UserId, 
    string Reason, 
    DateTime? SuspendUntil
) : IRequest<UserStatusResponse>;

/// <summary>
/// Command to activate a user.
/// </summary>
public record ActivateUserCommand(Guid UserId) : IRequest<UserStatusResponse>;

/// <summary>
/// Command to deactivate a user.
/// </summary>
public record DeactivateUserCommand(Guid UserId) : IRequest<UserStatusResponse>;

/// <summary>
/// Response for user status changes.
/// </summary>
public record UserStatusResponse(
    Guid UserId,
    string Status,
    string Message
);
