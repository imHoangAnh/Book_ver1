using BookStation.Application.Payments.Commands;
using BookStation.Application.Services;
using BookStation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Controller for payment processing.
/// Handles payment creation and VNPay callback.
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentService _paymentService;

    public PaymentsController(IMediator mediator, IPaymentService paymentService)
    {
        _mediator = mediator;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Process payment for an existing order.
    /// </summary>
    [HttpPost("{orderId:long}/process")]
    [Authorize]
    [ProducesResponseType(typeof(ProcessPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessPayment(long orderId, [FromBody] ProcessPaymentRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        var command = new ProcessPaymentCommand(
            orderId,
            request.PaymentMethod,
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
    /// VNPay return URL callback.
    /// This endpoint is called by VNPay after payment is completed.
    /// </summary>
    [HttpGet("vnpay-return")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VnPayReturnResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> VnPayReturn(
        [FromQuery] string vnp_TxnRef,
        [FromQuery] string vnp_Amount,
        [FromQuery] string vnp_BankCode,
        [FromQuery] string? vnp_BankTranNo,
        [FromQuery] string? vnp_CardType,
        [FromQuery] string vnp_OrderInfo,
        [FromQuery] string vnp_PayDate,
        [FromQuery] string vnp_ResponseCode,
        [FromQuery] string vnp_TmnCode,
        [FromQuery] string vnp_TransactionNo,
        [FromQuery] string vnp_TransactionStatus,
        [FromQuery] string vnp_SecureHash)
    {
        var command = new ConfirmVnPayPaymentCommand(
            vnp_TxnRef,
            vnp_Amount,
            vnp_BankCode,
            vnp_BankTranNo ?? "",
            vnp_CardType ?? "",
            vnp_OrderInfo,
            vnp_PayDate,
            vnp_ResponseCode,
            vnp_TmnCode,
            vnp_TransactionNo,
            vnp_TransactionStatus,
            vnp_SecureHash
        );

        var result = await _mediator.Send(command);

        // Return a JSON response that can be used by frontend
        var returnResult = new VnPayReturnResult(
            result.Success,
            result.OrderId,
            result.Success ? "Payment successful" : result.ErrorMessage,
            vnp_ResponseCode
        );

        // You can also redirect to a frontend page
        // return Redirect($"/payment-result?success={result.Success}&orderId={result.OrderId}");

        return Ok(returnResult);
    }

    /// <summary>
    /// VNPay IPN (Instant Payment Notification) callback.
    /// This is a server-to-server call from VNPay.
    /// </summary>
    [HttpPost("vnpay-ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayIpn(
        [FromQuery] string vnp_TxnRef,
        [FromQuery] string vnp_Amount,
        [FromQuery] string vnp_BankCode,
        [FromQuery] string? vnp_BankTranNo,
        [FromQuery] string? vnp_CardType,
        [FromQuery] string vnp_OrderInfo,
        [FromQuery] string vnp_PayDate,
        [FromQuery] string vnp_ResponseCode,
        [FromQuery] string vnp_TmnCode,
        [FromQuery] string vnp_TransactionNo,
        [FromQuery] string vnp_TransactionStatus,
        [FromQuery] string vnp_SecureHash)
    {
        var command = new ConfirmVnPayPaymentCommand(
            vnp_TxnRef,
            vnp_Amount,
            vnp_BankCode,
            vnp_BankTranNo ?? "",
            vnp_CardType ?? "",
            vnp_OrderInfo,
            vnp_PayDate,
            vnp_ResponseCode,
            vnp_TmnCode,
            vnp_TransactionNo,
            vnp_TransactionStatus,
            vnp_SecureHash
        );

        var result = await _mediator.Send(command);

        // VNPay expects specific response format
        if (result.Success)
        {
            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }
        else
        {
            return Ok(new { RspCode = "99", Message = result.ErrorMessage ?? "Confirm Failed" });
        }
    }

    /// <summary>
    /// Get payment status for an order.
    /// </summary>
    [HttpGet("{orderId:long}/status")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentStatus(long orderId)
    {
        try
        {
            var result = await _paymentService.GetPaymentStatusAsync(orderId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

// =====================================================
// REQUEST/RESPONSE DTOs
// =====================================================

public record ProcessPaymentRequest(
    EPaymentMethod PaymentMethod,
    string? ReturnUrl = null     // Required for VNPay
);

public record VnPayReturnResult(
    bool Success,
    long OrderId,
    string? Message,
    string ResponseCode
);
