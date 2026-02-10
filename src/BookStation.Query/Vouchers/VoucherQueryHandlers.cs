using BookStation.Domain.Entities.VoucherAggregate;
using BookStation.Domain.Enums;
using BookStation.Domain.ValueObjects;
using BookStation.Query.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Query.Vouchers;

/// <summary>
/// Handler for GetVoucherByIdQuery.
/// </summary>
public class GetVoucherByIdQueryHandler : IRequestHandler<GetVoucherByIdQuery, VoucherDetailDto?>
{
    private readonly IReadDbContext _dbContext;

    public GetVoucherByIdQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VoucherDetailDto?> Handle(
        GetVoucherByIdQuery request,
        CancellationToken cancellationToken)
    {
        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(v => v.Id == request.VoucherId, cancellationToken);

        return voucher != null ? MapToDto(voucher) : null;
    }

    private static VoucherDetailDto MapToDto(Voucher v) => new(
        v.Id, v.Code, v.Name, v.DiscountType, v.DiscountValue,
        v.MinOrderAmount.Amount, v.MaxDiscountAmount?.Amount,
        v.UsageLimit, v.UsageCount, v.RemainingUses,
        v.StartDate, v.EndDate, v.IsActive, v.IsValid(), v.OrganizationId
    );
}

/// <summary>
/// Handler for GetVoucherByCodeQuery.
/// </summary>
public class GetVoucherByCodeQueryHandler : IRequestHandler<GetVoucherByCodeQuery, VoucherDetailDto?>
{
    private readonly IReadDbContext _dbContext;

    public GetVoucherByCodeQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VoucherDetailDto?> Handle(
        GetVoucherByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.ToUpperInvariant().Trim();
        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(v => v.Code == code, cancellationToken);

        return voucher != null ? MapToDto(voucher) : null;
    }

    private static VoucherDetailDto MapToDto(Voucher v) => new(
        v.Id, v.Code, v.Name, v.DiscountType, v.DiscountValue,
        v.MinOrderAmount.Amount, v.MaxDiscountAmount?.Amount,
        v.UsageLimit, v.UsageCount, v.RemainingUses,
        v.StartDate, v.EndDate, v.IsActive, v.IsValid(), v.OrganizationId
    );
}

/// <summary>
/// Handler for GetVouchersQuery.
/// </summary>
public class GetVouchersQueryHandler : IRequestHandler<GetVouchersQuery, VouchersListDto>
{
    private readonly IReadDbContext _dbContext;

    public GetVouchersQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VouchersListDto> Handle(
        GetVouchersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Vouchers.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(v => v.IsActive == request.IsActive.Value);
        }

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(v => v.OrganizationId == request.OrganizationId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var vouchers = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VoucherDetailDto(
                v.Id, v.Code, v.Name, v.DiscountType, v.DiscountValue,
                v.MinOrderAmount.Amount, v.MaxDiscountAmount != null ? v.MaxDiscountAmount.Amount : null,
                v.UsageLimit, v.UsageCount, v.UsageLimit > 0 ? v.UsageLimit - v.UsageCount : int.MaxValue,
                v.StartDate, v.EndDate, v.IsActive, v.IsActive && DateTime.UtcNow >= v.StartDate && DateTime.UtcNow <= v.EndDate, v.OrganizationId
            ))
            .ToListAsync(cancellationToken);

        return new VouchersListDto(vouchers, totalCount, request.PageNumber, request.PageSize, totalPages);
    }
}

/// <summary>
/// Handler for GetAvailableVouchersQuery.
/// </summary>
public class GetAvailableVouchersQueryHandler : IRequestHandler<GetAvailableVouchersQuery, AvailableVouchersDto>
{
    private readonly IReadDbContext _dbContext;

    public GetAvailableVouchersQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AvailableVouchersDto> Handle(
        GetAvailableVouchersQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var orderAmount = request.OrderAmount;

        var vouchers = await _dbContext.Vouchers
            .Where(v => v.IsActive)
            .Where(v => v.StartDate <= now && v.EndDate >= now)
            .Where(v => v.MinOrderAmount.Amount <= orderAmount)
            .Where(v => v.UsageLimit == 0 || v.UsageCount < v.UsageLimit)
            .ToListAsync(cancellationToken);

        // Filter out vouchers already used by this user if userId provided
        if (request.UserId.HasValue)
        {
            vouchers = vouchers
                .Where(v => !v.HasUserUsed(request.UserId.Value))
                .ToList();
        }

        var orderAmountMoney = Money.Create(orderAmount);
        var result = vouchers.Select(v => new AvailableVoucherDto(
            v.Id, v.Code, v.Name, v.DiscountType, v.DiscountValue,
            v.MinOrderAmount.Amount, v.MaxDiscountAmount?.Amount, v.EndDate,
            v.CalculateDiscount(orderAmountMoney).Amount
        )).OrderByDescending(v => v.CalculatedDiscount).ToList();

        return new AvailableVouchersDto(result);
    }
}

/// <summary>
/// Handler for ValidateVoucherQuery.
/// </summary>
public class ValidateVoucherQueryHandler : IRequestHandler<ValidateVoucherQuery, VoucherValidationDto>
{
    private readonly IReadDbContext _dbContext;

    public ValidateVoucherQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VoucherValidationDto> Handle(
        ValidateVoucherQuery request,
        CancellationToken cancellationToken)
    {
        var code = request.VoucherCode.ToUpperInvariant().Trim();
        var voucher = await _dbContext.Vouchers
            .Include(v => v.Usages)
            .FirstOrDefaultAsync(v => v.Code == code, cancellationToken);

        if (voucher == null)
        {
            return new VoucherValidationDto(false, null, null, 0, "Mã voucher không tồn tại.");
        }

        if (!voucher.IsValid())
        {
            return new VoucherValidationDto(false, voucher.Id, voucher.Name, 0, "Voucher đã hết hạn hoặc không còn hiệu lực.");
        }

        var orderAmount = Money.Create(request.OrderAmount);
        if (!voucher.CanApply(orderAmount))
        {
            return new VoucherValidationDto(false, voucher.Id, voucher.Name, 0, 
                $"Đơn hàng tối thiểu phải từ {voucher.MinOrderAmount.Amount:N0} VND.");
        }

        if (voucher.HasUserUsed(request.UserId))
        {
            return new VoucherValidationDto(false, voucher.Id, voucher.Name, 0, "Bạn đã sử dụng voucher này rồi.");
        }

        var discount = voucher.CalculateDiscount(orderAmount);
        return new VoucherValidationDto(true, voucher.Id, voucher.Name, discount.Amount);
    }
}
