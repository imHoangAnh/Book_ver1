using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Enums;

namespace BookStation.Application.Services;

/// <summary>
/// Service interface for payment processing.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Processes a cash payment (COD).
    /// </summary>
    Task<PaymentResult> ProcessCashPaymentAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a VNPay payment URL.
    /// </summary>
    Task<VnPayPaymentResult> CreateVnPayPaymentUrlAsync(Order order, string returnUrl, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes VNPay callback/return.
    /// </summary>
    Task<PaymentResult> ProcessVnPayReturnAsync(VnPayReturnRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment status by order ID.
    /// </summary>
    Task<PaymentStatusResult> GetPaymentStatusAsync(long orderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment processing result.
/// </summary>
public record PaymentResult(
    bool Success,
    long PaymentId,
    string? TransactionId,
    string? ErrorMessage = null
);

/// <summary>
/// VNPay payment URL result.
/// </summary>
public record VnPayPaymentResult(
    bool Success,
    string? PaymentUrl,
    long PaymentId,
    string? ErrorMessage = null
);

/// <summary>
/// VNPay return request data.
/// </summary>
public record VnPayReturnRequest(
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
);

/// <summary>
/// Payment status result.
/// </summary>
public record PaymentStatusResult(
    long OrderId,
    EPaymentStatus Status,
    decimal TotalAmount,
    decimal PaidAmount,
    bool IsFullyPaid,
    List<PaymentInfo> Payments
);

/// <summary>
/// Individual payment information.
/// </summary>
public record PaymentInfo(
    long PaymentId,
    decimal Amount,
    EPaymentMethod Method,
    EPaymentStatus Status,
    string? TransactionId,
    DateTime? CompletedAt
);
