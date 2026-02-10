using BookStation.Application.Services;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Handler for UpdateProfileCommand.
/// Supports both URL-based avatar and file upload.
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IImageUploadService _imageUploadService;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IImageUploadService imageUploadService)
    {
        _userRepository = userRepository;
        _imageUploadService = imageUploadService;
    }

    public async Task<UpdateProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Handle avatar upload if file stream is provided
        string? avatarUrl = request.AvatarUrl;
        if (request.AvatarStream != null && !string.IsNullOrWhiteSpace(request.AvatarFileName))
        {
            var uploadResult = await _imageUploadService.UploadAvatarAsync(
                request.AvatarStream,
                request.AvatarFileName,
                cancellationToken
            );

            if (uploadResult.Success)
            {
                avatarUrl = uploadResult.Url;
            }
            else
            {
                throw new InvalidOperationException($"Failed to upload avatar: {uploadResult.Error}");
            }
        }

        // Create PhoneNumber value object if provided
        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            phone = PhoneNumber.Create(request.Phone);
        }

        // Create Address value object if provided
        Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.Street) && 
            !string.IsNullOrWhiteSpace(request.City) && 
            !string.IsNullOrWhiteSpace(request.Country))
        {
            address = Address.Create(
                request.Street,
                request.City,
                request.Country,
                request.Ward,
                request.District,
                request.PostalCode
            );
        }

        user.UpdateProfile(
            request.FullName, 
            phone, 
            request.Bio, 
            avatarUrl, 
            address,
            request.Gender,
            request.DateOfBirth
        );

        return new UpdateProfileResponse(
            user.Id,
            user.FullName,
            user.Phone?.Value,
            user.Bio,
            user.AvatarUrl,
            user.Address?.FullAddress,
            user.Gender?.ToString(),
            user.DateOfBirth,
            "Profile updated successfully."
        );
    }
}

/// <summary>
/// Handler for ChangePasswordCommand.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Verify current password
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect.");

        // Validate and create Password value object for new password
        // This enforces password strength rules at Domain level
        var newPassword = Password.Create(request.NewPassword);

        // Hash new password - returns PasswordHash value object
        var newPasswordHash = _passwordHasher.HashPassword(newPassword);
        
        user.ChangePassword(newPasswordHash);

        return new ChangePasswordResponse(true, "Password changed successfully.");
    }
}
