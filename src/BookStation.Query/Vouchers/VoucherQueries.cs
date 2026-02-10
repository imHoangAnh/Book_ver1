using BookStation.Domain.Enums;
using MediatR;

namespace BookStation.Query.Vouchers;

/// <summary>
/// Query to get voucher by ID.
/// </summary>
public record GetVoucherByIdQuery(long VoucherId) : IRequest<VoucherDetailDto?>;

/// <summary>
/// Query to get voucher by code.
/// </summary>
public record GetVoucherByCodeQuery(string Code) : IRequest<VoucherDetailDto?>;

/// <summary>
/// Query to get all vouchers with filtering.
/// </summary>
public record GetVouchersQuery(
    bool? IsActive = null,
    int? OrganizationId = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<VouchersListDto>;

/// <summary>
/// Query to get available vouchers for a specific order amount.
/// </summary>
public record GetAvailableVouchersQuery(
    decimal OrderAmount,
    long? UserId = null
) : IRequest<AvailableVouchersDto>;

/// <summary>
/// Query to validate voucher for an order.
/// </summary>
public record ValidateVoucherQuery(
    string VoucherCode,
    decimal OrderAmount,
    long UserId
) : IRequest<VoucherValidationDto>;

/// <summary>
/// Voucher detail DTO.
/// </summary>
public record VoucherDetailDto(
    long Id,
    string Code,
    string Name,
    EDiscountType DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    int UsageLimit,
    int UsageCount,
    int RemainingUses,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsValid,
    int? OrganizationId
);

/// <summary>
/// Vouchers list response DTO.
/// </summary>
public record VouchersListDto(
    List<VoucherDetailDto> Vouchers,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Available vouchers DTO with calculated discount.
/// </summary>
public record AvailableVouchersDto(
    List<AvailableVoucherDto> Vouchers
);

/// <summary>
/// Available voucher with calculated discount.
/// </summary>
public record AvailableVoucherDto(
    long Id,
    string Code,
    string Name,
    EDiscountType DiscountType,
    decimal DiscountValue,
    decimal MinOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime EndDate,
    decimal CalculatedDiscount  // Pre-calculated discount for the order amount
);

/// <summary>
/// Voucher validation result DTO.
/// </summary>
public record VoucherValidationDto(
    bool IsValid,
    long? VoucherId,
    string? VoucherName,
    decimal DiscountAmount,
    string? ErrorMessage = null
);
