using BookStation.Domain.Enums;
using MediatR;

namespace BookStation.Application.Orders.Commands;

/// <summary>
/// Command to checkout an order with shipping address and payment method.
/// </summary>
public record CheckoutCommand(
    Guid UserId,
    List<CheckoutItemDto> Items,
    int? ShippingAddressId,      // If null, use default address
    EPaymentMethod PaymentMethod,
    string? VoucherCode = null,
    string? Notes = null,
    string? ReturnUrl = null,    // Required for online payment
    string? IpAddress = null
) : IRequest<CheckoutResponse>;

/// <summary>
/// Item in the checkout.
/// </summary>
public record CheckoutItemDto(
    long VariantId,
    int Quantity
);

/// <summary>
/// Response for checkout.
/// </summary>
public record CheckoutResponse(
    bool Success,
    long? OrderId,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    EPaymentMethod PaymentMethod,
    EPaymentStatus PaymentStatus,
    string? PaymentUrl = null,      // For VNPay redirect
    string? ErrorMessage = null
);

/// <summary>
/// Query to get available vouchers for user.
/// </summary>
public record GetAvailableVouchersQuery(
    Guid UserId,
    decimal OrderAmount
) : IRequest<AvailableVouchersResponse>;

/// <summary>
/// Response for available vouchers.
/// </summary>
public record AvailableVouchersResponse(
    List<VoucherDto> Vouchers
);

/// <summary>
/// Voucher DTO.
/// </summary>
public record VoucherDto(
    long Id,
    string Code,
    string Name,
    EDiscountType DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    decimal CalculatedDiscount  // Pre-calculated discount for the order amount
);

/// <summary>
/// Command to validate and preview voucher for an order amount.
/// </summary>
public record ValidateVoucherCommand(
    string VoucherCode,
    decimal OrderAmount,
    Guid UserId
) : IRequest<ValidateVoucherResponse>;

/// <summary>
/// Response for voucher validation.
/// </summary>
public record ValidateVoucherResponse(
    bool IsValid,
    long? VoucherId,
    string? VoucherName,
    decimal DiscountAmount,
    string? ErrorMessage = null
);
