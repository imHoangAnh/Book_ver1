using BookStation.Domain.Enums;
using MediatR;

namespace BookStation.Application.Payments.Commands;

/// <summary>
/// Command to process payment for an order.
/// </summary>
public record ProcessPaymentCommand(
    long OrderId,
    EPaymentMethod PaymentMethod,
    string? ReturnUrl = null,  // Required for VNPay
    string? IpAddress = null   // Required for VNPay
) : IRequest<ProcessPaymentResponse>;

/// <summary>
/// Response for payment processing.
/// </summary>
public record ProcessPaymentResponse(
    bool Success,
    long PaymentId,
    EPaymentMethod PaymentMethod,
    EPaymentStatus PaymentStatus,
    string? PaymentUrl = null,      // For VNPay - redirect user to this URL
    string? TransactionId = null,
    string? ErrorMessage = null
);

/// <summary>
/// Command to confirm VNPay payment return.
/// </summary>
public record ConfirmVnPayPaymentCommand(
    string VnpTxnRef,
    string VnpAmount,
    string VnpBankCode,
    string VnpBankTranNo,
    string VnpCardType,
    string VnpOrderInfo,
    string VnpPayDate,
    string VnpResponseCode,
    string VnpTmnCode,
    string VnpTransactionNo,
    string VnpTransactionStatus,
    string VnpSecureHash
) : IRequest<ConfirmVnPayPaymentResponse>;

/// <summary>
/// Response for VNPay payment confirmation.
/// </summary>
public record ConfirmVnPayPaymentResponse(
    bool Success,
    long OrderId,
    long PaymentId,
    string? TransactionId,
    string? ErrorMessage = null
);
