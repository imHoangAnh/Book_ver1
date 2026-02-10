using BookStation.Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Admin controller for managing seller applications.
/// Handles listing pending sellers, approving and rejecting seller requests.
/// </summary>
[ApiController]
[Route("api/sellers")]
[Authorize(Roles = "Admin")]
public class SellersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all pending seller applications.
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingSellers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Implement GetPendingSellersQuery
        return Ok(new 
        { 
            message = "Pending sellers list - to be implemented",
            pagination = new { page, pageSize }
        });
    }

    /// <summary>
    /// Get all sellers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSellers(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Implement GetSellersQuery
        return Ok(new 
        { 
            message = "Sellers list - to be implemented",
            filters = new { status, page, pageSize }
        });
    }

    /// <summary>
    /// Get seller details by user ID.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSellerById(Guid userId)
    {
        // TODO: Implement GetSellerByIdQuery
        return Ok(new { userId, message = "Seller details - to be implemented" });
    }

    /// <summary>
    /// Approve a seller application.
    /// </summary>
    [HttpPost("{userId:guid}/approve")]
    [ProducesResponseType(typeof(SellerStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveSeller(Guid userId)
    {
        try
        {
            var command = new ApproveSellerCommand(userId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reject a seller application.
    /// </summary>
    [HttpPost("{userId:guid}/reject")]
    [ProducesResponseType(typeof(SellerStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectSeller(Guid userId, [FromBody] RejectSellerRequest request)
    {
        try
        {
            var command = new RejectSellerCommand(userId, request.Reason);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Suspend a seller.
    /// </summary>
    [HttpPost("{userId:guid}/suspend")]
    [ProducesResponseType(typeof(SellerStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendSeller(Guid userId, [FromBody] SuspendSellerRequest request)
    {
        try
        {
            var command = new SuspendSellerCommand(userId, request.Reason);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reactivate a suspended seller.
    /// </summary>
    [HttpPost("{userId:guid}/reactivate")]
    [ProducesResponseType(typeof(SellerStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateSeller(Guid userId)
    {
        try
        {
            var command = new ReactivateSellerCommand(userId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

// Request DTOs
public record RejectSellerRequest(string Reason);
public record SuspendSellerRequest(string Reason);
