using BookStation.Application.Orders.Commands;
using BookStation.Application.Payments.Commands;
using BookStation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Controller for checkout and payment processing.
/// Handles order checkout with address selection, payment method, and vouchers.
/// </summary>
[ApiController]
[Route("api/checkout")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Checkout an order with items, shipping address, payment method, and optional voucher.
    /// </summary>
    /// <remarks>
    /// If ShippingAddressId is not provided, the user's default address will be used.
    /// For VNPay payment, provide a ReturnUrl where users will be redirected after payment.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        // Get client IP
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        var command = new CheckoutCommand(
            userId.Value,
            request.Items.Select(i => new CheckoutItemDto(i.VariantId, i.Quantity)).ToList(),
            request.ShippingAddressId,
            request.PaymentMethod,
            request.VoucherCode,
            request.Notes,
            request.ReturnUrl,
            ipAddress
        );

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    /// <summary>
    /// Validate a voucher code for a specific order amount.
    /// </summary>
    [HttpPost("validate-voucher")]
    [ProducesResponseType(typeof(ValidateVoucherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var command = new ValidateVoucherCommand(
            request.VoucherCode,
            request.OrderAmount,
            userId.Value
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get available vouchers for the current order amount.
    /// </summary>
    [HttpGet("vouchers")]
    [ProducesResponseType(typeof(AvailableVouchersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableVouchers([FromQuery] decimal orderAmount)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var query = new GetAvailableVouchersQuery(userId.Value, orderAmount);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

// =====================================================
// REQUEST DTOs
// =====================================================

public record CheckoutRequest(
    List<CheckoutItemRequest> Items,
    int? ShippingAddressId,                     // If null, use default address
    EPaymentMethod PaymentMethod,               // Cash = 0, VNPay = 4
    string? VoucherCode = null,
    string? Notes = null,
    string? ReturnUrl = null                    // Required for VNPay
);

public record CheckoutItemRequest(
    long VariantId,
    int Quantity
);

public record ValidateVoucherRequest(
    string VoucherCode,
    decimal OrderAmount
);
