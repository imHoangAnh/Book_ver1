using BookStation.Domain.Enums;
using MediatR;

namespace BookStation.Application.Vouchers.Commands;

/// <summary>
/// Command to create a new voucher (Admin/Seller only).
/// </summary>
public record CreateVoucherCommand(
    string Code,
    string Name,
    EDiscountType DiscountType,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int UsageLimit = 0,
    Guid? SellerId = null
) : IRequest<CreateVoucherResponse>;

/// <summary>
/// Response for creating voucher.
/// </summary>
public record CreateVoucherResponse(
    long VoucherId,
    string Code,
    string Name,
    bool IsActive
);

/// <summary>
/// Command to update a voucher.
/// </summary>
public record UpdateVoucherCommand(
    long VoucherId,
    string Name,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int UsageLimit = 0
) : IRequest<UpdateVoucherResponse>;

/// <summary>
/// Response for updating voucher.
/// </summary>
public record UpdateVoucherResponse(
    bool Success,
    string? ErrorMessage = null
);

/// <summary>
/// Command to activate/deactivate a voucher.
/// </summary>
public record SetVoucherStatusCommand(
    long VoucherId,
    bool IsActive
) : IRequest<SetVoucherStatusResponse>;

/// <summary>
/// Response for setting voucher status.
/// </summary>
public record SetVoucherStatusResponse(
    bool Success,
    string? ErrorMessage = null
);

/// <summary>
/// Query to get voucher by ID.
/// </summary>
public record GetVoucherByIdQuery(
    long VoucherId
) : IRequest<VoucherDetailDto?>;

/// <summary>
/// Query to get all vouchers with filtering.
/// </summary>
public record GetVouchersQuery(
    bool? IsActive = null,
    Guid? SellerId = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<VouchersListResponse>;

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
    Guid? SellerId
);

/// <summary>
/// Response for vouchers list.
/// </summary>
public record VouchersListResponse(
    List<VoucherDetailDto> Vouchers,
    int TotalCount,
    int PageNumber,
    int PageSize
);
