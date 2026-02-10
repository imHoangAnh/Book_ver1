using BookStation.Domain.Enums;
using MediatR;

namespace BookStation.Query.Payments;

/// <summary>
/// Query to get payment status for an order.
/// </summary>
public record GetPaymentStatusQuery(long OrderId) : IRequest<PaymentStatusDto?>;

/// <summary>
/// Payment status DTO.
/// </summary>
public record PaymentStatusDto(
    long OrderId,
    string OrderStatus,
    decimal TotalAmount,
    decimal PaidAmount,
    bool IsFullyPaid,
    List<PaymentDetailDto> Payments
);

/// <summary>
/// Payment detail DTO.
/// </summary>
public record PaymentDetailDto(
    long Id,
    decimal Amount,
    string Method,
    string Status,
    string? TransactionId,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

/// <summary>
/// Query to get order payments history.
/// </summary>
public record GetOrderPaymentsQuery(long OrderId) : IRequest<OrderPaymentsDto?>;

/// <summary>
/// Order payments DTO.
/// </summary>
public record OrderPaymentsDto(
    long OrderId,
    decimal TotalAmount,
    decimal FinalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    List<PaymentDetailDto> Payments
);
