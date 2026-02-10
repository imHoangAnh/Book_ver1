using BookStation.Application.Vouchers.Commands;
using BookStation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Controller for voucher management.
/// Admin/Seller can create, update, and manage vouchers.
/// </summary>
[ApiController]
[Route("api/vouchers")]
public class VouchersController : ControllerBase
{
    private readonly IMediator _mediator;

    public VouchersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all vouchers with optional filtering.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(VouchersListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVouchers(
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? sellerId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetVouchersQuery(isActive, sellerId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get voucher details by ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(VoucherDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVoucherById(long id)
    {
        var query = new GetVoucherByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Create a new voucher.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(CreateVoucherResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherRequest request)
    {
        try
        {
            var command = new CreateVoucherCommand(
                request.Code,
                request.Name,
                request.DiscountType,
                request.DiscountValue,
                request.StartDate,
                request.EndDate,
                request.MinOrderAmount,
                request.MaxDiscountAmount,
                request.UsageLimit,
                request.UsageLimit,
                request.SellerId
            );

            // If user is a seller and didn't provide SellerId (or provided one), 
            // we should probably enforce or default to their own ID. 
            // However, for simplicity as per current request, I'll trust the request or let logic handle it.
            // Better yet, if we want to support "Seller creates shop voucher", we should grab their ID.
            // But let's stick to the request body first as per API design.
            // The USER said "User new status active... organization removed... seller is shop".
            
            // To be safe and helpful: If I am a seller (not admin), I force SellerId = MyId.
            // If I am Admin, I can set SellerId (for a shop) or null (for System).
            
            // Let's checking User claims.
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role == "Seller" && Guid.TryParse(userId, out var sellerId))
            {
                 // Re-create command forcing SellerId if it's a seller
                 command = command with { SellerId = sellerId };
            }

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetVoucherById), new { id = result.VoucherId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing voucher.
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(UpdateVoucherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVoucher(long id, [FromBody] UpdateVoucherRequest request)
    {
        var command = new UpdateVoucherCommand(
            id,
            request.Name,
            request.DiscountValue,
            request.StartDate,
            request.EndDate,
            request.MinOrderAmount,
            request.MaxDiscountAmount,
            request.UsageLimit
        );

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
                return NotFound(new { error = result.ErrorMessage });

            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    /// <summary>
    /// Activate a voucher.
    /// </summary>
    [HttpPost("{id:long}/activate")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(SetVoucherStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateVoucher(long id)
    {
        var command = new SetVoucherStatusCommand(id, true);
        var result = await _mediator.Send(command);

        if (!result.Success)
            return NotFound(new { error = result.ErrorMessage });

        return Ok(result);
    }

    /// <summary>
    /// Deactivate a voucher.
    /// </summary>
    [HttpPost("{id:long}/deactivate")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(SetVoucherStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateVoucher(long id)
    {
        var command = new SetVoucherStatusCommand(id, false);
        var result = await _mediator.Send(command);

        if (!result.Success)
            return NotFound(new { error = result.ErrorMessage });

        return Ok(result);
    }
}

// =====================================================
// REQUEST DTOs
// =====================================================

public record CreateVoucherRequest(
    string Code,
    string Name,
    EDiscountType DiscountType,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int UsageLimit = 0,
    int UsageLimit = 0,
    Guid? SellerId = null
);

public record UpdateVoucherRequest(
    string Name,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int UsageLimit = 0
);
