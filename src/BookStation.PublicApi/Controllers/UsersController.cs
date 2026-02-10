using BookStation.Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Admin controller for managing users.
/// Handles user listing and status management (verify, ban, suspend, activate).
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all users with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Implement GetUsersQuery with filtering
        return Ok(new 
        { 
            message = "List users endpoint - to be implemented",
            filters = new { status, role, page, pageSize }
        });
    }

    /// <summary>
    /// Get user by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // TODO: Implement GetUserByIdQuery
        return Ok(new { id, message = "User details endpoint - to be implemented" });
    }

    /// <summary>
    /// Verify a pending user (Pending → Active).
    /// </summary>
    [HttpPost("{id:guid}/verify")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyUser(Guid id)
    {
        try
        {
            var command = new VerifyUserCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ban a user account.
    /// </summary>
    [HttpPost("{id:guid}/ban")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BanUser(Guid id, [FromBody] BanUserRequest request)
    {
        try
        {
            var command = new BanUserCommand(id, request.Reason);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Unban a user account.
    /// </summary>
    [HttpPost("{id:guid}/unban")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnbanUser(Guid id)
    {
        try
        {
            var command = new UnbanUserCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Suspend a user account temporarily.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserRequest request)
    {
        try
        {
            var command = new SuspendUserCommand(id, request.Reason, request.SuspendUntil);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Activate a user account.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {
            var command = new ActivateUserCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a user account.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(UserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var command = new DeactivateUserCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // =====================================================
    // AUTHOR PROFILE ENDPOINTS
    // =====================================================

    /// <summary>
    /// User claims they are an author from the catalog (creates AuthorProfile).
    /// Requires verification from admin to get blue tick.
    /// </summary>
    [HttpPost("{id:guid}/author-profile")]
    [ProducesResponseType(typeof(BecomeAuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BecomeAuthor(Guid id, [FromBody] BecomeAuthorRequest request)
    {
        try
        {
            var command = new BecomeAuthorCommand(id, request.AuthorId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Admin verifies or revokes verification of a user's author profile (blue tick).
    /// </summary>
    [HttpPatch("{id:guid}/author-profile/verification")]
    [ProducesResponseType(typeof(VerifyAuthorProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyAuthorProfile(Guid id, [FromBody] VerifyAuthorProfileRequest request)
    {
        try
        {
            var command = new VerifyAuthorProfileCommand(id, request.IsVerified);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Request DTOs
public record BanUserRequest(string Reason);
public record SuspendUserRequest(string Reason, DateTime? SuspendUntil);
public record BecomeAuthorRequest(long AuthorId);
public record VerifyAuthorProfileRequest(bool IsVerified);
