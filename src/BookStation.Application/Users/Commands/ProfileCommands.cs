using BookStation.Domain.Enums;
using MediatR;

namespace BookStation.Application.Users.Commands;

// =====================================================
// AUTH COMMANDS (for AuthController)
// =====================================================

/// <summary>
/// Command to update user profile.
/// Supports both URL-based avatar (AvatarUrl) and file upload (AvatarStream).
/// If AvatarStream is provided, it will be uploaded to Cloudinary and override AvatarUrl.
/// </summary>
public record UpdateProfileCommand(
    Guid UserId,
    string? FullName,
    string? Phone,
    string? Bio,
    string? AvatarUrl,
    string? Street,
    string? City,
    string? Country,
    string? Ward,
    string? District,
    string? PostalCode,
    EGender? Gender = null,
    DateOnly? DateOfBirth = null,
    // Avatar file upload support
    Stream? AvatarStream = null,
    string? AvatarFileName = null
) : IRequest<UpdateProfileResponse>;

/// <summary>
/// Response for profile update.
/// </summary>
public record UpdateProfileResponse(
    Guid UserId,
    string? FullName,
    string? Phone,
    string? Bio,
    string? AvatarUrl,
    string? Address,
    string? Gender,
    DateOnly? DateOfBirth,
    string Message
);

/// <summary>
/// Command to change user password.
/// </summary>
public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<ChangePasswordResponse>;

/// <summary>
/// Response for password change.
/// </summary>
public record ChangePasswordResponse(
    bool Success,
    string Message
);
