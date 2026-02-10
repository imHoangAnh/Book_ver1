using BookStation.Application.Users.Commands;
using BookStation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Authentication controller for all users.
/// Handles registration, login, profile management, password change, and address management.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("/register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetProfile), new { }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Login user and get JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user profile (requires authentication).
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst("FullName")?.Value;
        var phone = User.FindFirst("Phone")?.Value;

        return Ok(new
        {
            userId,
            email,
            fullName,
            phone
        });
    }

    /// <summary>
    /// Update current user profile (JSON body, no file upload).
    /// Use PUT /api/auth/profile/with-avatar for avatar file upload.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var command = new UpdateProfileCommand(
                userId.Value, 
                request.FullName, 
                request.Phone,
                request.Bio,
                request.AvatarUrl,
                request.Street,
                request.City,
                request.Country,
                request.Ward,
                request.District,
                request.PostalCode,
                request.Gender,
                request.DateOfBirth
            );
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update current user profile with avatar file upload (multipart/form-data).
    /// </summary>
    [HttpPut("profile/with-avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfileWithAvatar([FromForm] UpdateProfileWithAvatarRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        // Validate avatar file if provided
        Stream? avatarStream = null;
        string? avatarFileName = null;

        if (request.AvatarFile != null && request.AvatarFile.Length > 0)
        {
            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(request.AvatarFile.ContentType.ToLower()))
                return BadRequest(new { error = "Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed." });

            // Validate file size (max 5MB)
            if (request.AvatarFile.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "File size exceeds the limit of 5MB." });

            avatarStream = request.AvatarFile.OpenReadStream();
            avatarFileName = request.AvatarFile.FileName;
        }

        try
        {
            var command = new UpdateProfileCommand(
                userId.Value, 
                request.FullName, 
                request.Phone,
                request.Bio,
                request.AvatarUrl,
                request.Street,
                request.City,
                request.Country,
                request.Ward,
                request.District,
                request.PostalCode,
                request.Gender,
                request.DateOfBirth,
                avatarStream,
                avatarFileName
            );
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        finally
        {
            avatarStream?.Dispose();
        }
    }

    /// <summary>
    /// Change current user password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { error = "New password and confirm password do not match." });

        try
        {
            var command = new ChangePasswordCommand(userId.Value, request.CurrentPassword, request.NewPassword);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // =====================================================
    // ADDRESS ENDPOINTS
    // =====================================================

    /// <summary>
    /// Create a new address for the current user.
    /// </summary>
    [HttpPost("address/new")]
    [Authorize]
    [ProducesResponseType(typeof(CreateAddressResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var command = new CreateAddressCommand(
                userId.Value,
                request.Street,
                request.City,
                request.Country,
                request.Ward,
                request.District,
                request.PostalCode,
                request.Label,
                request.IsDefault,
                request.RecipientName,
                request.RecipientPhone
            );
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAddresses), null, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all addresses for the current user.
    /// </summary>
    [HttpGet("address/index")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var query = new GetAddressesQuery(userId.Value);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing address.
    /// </summary>
    [HttpPut("address/{addressId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(UpdateAddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UpdateAddressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var command = new UpdateAddressCommand(
                userId.Value,
                addressId,
                request.Street,
                request.City,
                request.Country,
                request.Ward,
                request.District,
                request.PostalCode,
                request.Label,
                request.RecipientName,
                request.RecipientPhone
            );
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an address.
    /// </summary>
    [HttpDelete("address/{addressId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(DeleteAddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAddress(int addressId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var command = new DeleteAddressCommand(userId.Value, addressId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Set an address as default.
    /// </summary>
    [HttpPost("address/{addressId:int}/set-default")]
    [Authorize]
    [ProducesResponseType(typeof(SetDefaultAddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetDefaultAddress(int addressId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var command = new SetDefaultAddressCommand(userId.Value, addressId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // =====================================================
    // HELPER METHODS
    // =====================================================

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

// =====================================================
// REQUEST DTOs
// =====================================================

public record UpdateProfileRequest(
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
    DateOnly? DateOfBirth = null
);

/// <summary>
/// Request DTO for updating profile with avatar file upload.
/// </summary>
public class UpdateProfileWithAvatarRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public IFormFile? AvatarFile { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public EGender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

public record ChangePasswordRequest(
    string CurrentPassword, 
    string NewPassword, 
    string ConfirmPassword
);

public record CreateAddressRequest(
    string Street,
    string City,
    string Country,
    string? Ward = null,
    string? District = null,
    string? PostalCode = null,
    string? Label = "Home",
    bool IsDefault = false,
    string? RecipientName = null,
    string? RecipientPhone = null
);

public record UpdateAddressRequest(
    string Street,
    string City,
    string Country,
    string? Ward = null,
    string? District = null,
    string? PostalCode = null,
    string? Label = null,
    string? RecipientName = null,
    string? RecipientPhone = null
);

