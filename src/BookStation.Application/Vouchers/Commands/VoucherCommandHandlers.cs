using BookStation.Domain.Entities.VoucherAggregate;
using BookStation.Domain.Enums;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Vouchers.Commands;

/// <summary>
/// Handler for CreateVoucherCommand.
/// </summary>
public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, CreateVoucherResponse>
{
    private readonly IVoucherRepository _voucherRepository;

    public CreateVoucherCommandHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<CreateVoucherResponse> Handle(
        CreateVoucherCommand request,
        CancellationToken cancellationToken)
    {
        // Check if code is unique
        var isUnique = await _voucherRepository.IsCodeUniqueAsync(request.Code, cancellationToken);
        if (!isUnique)
        {
            throw new InvalidOperationException($"Voucher code '{request.Code}' already exists.");
        }

        var voucher = Voucher.Create(
            request.Code,
            request.Name,
            request.DiscountType,
            request.DiscountValue,
            request.StartDate,
            request.EndDate,
            request.MinOrderAmount.HasValue ? Money.Create(request.MinOrderAmount.Value) : null,
            request.MaxDiscountAmount.HasValue ? Money.Create(request.MaxDiscountAmount.Value) : null,
            request.UsageLimit,
            request.SellerId
        );

        await _voucherRepository.AddAsync(voucher, cancellationToken);

        return new CreateVoucherResponse(
            voucher.Id,
            voucher.Code,
            voucher.Name,
            voucher.IsActive
        );
    }
}

/// <summary>
/// Handler for UpdateVoucherCommand.
/// </summary>
public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, UpdateVoucherResponse>
{
    private readonly IVoucherRepository _voucherRepository;

    public UpdateVoucherCommandHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<UpdateVoucherResponse> Handle(
        UpdateVoucherCommand request,
        CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher == null)
        {
            return new UpdateVoucherResponse(false, "Voucher not found.");
        }

        voucher.Update(
            request.Name,
            request.DiscountValue,
            request.StartDate,
            request.EndDate,
            request.MinOrderAmount.HasValue ? Money.Create(request.MinOrderAmount.Value) : null,
            request.MaxDiscountAmount.HasValue ? Money.Create(request.MaxDiscountAmount.Value) : null,
            request.UsageLimit
        );

        await _voucherRepository.UpdateAsync(voucher, cancellationToken);

        return new UpdateVoucherResponse(true);
    }
}

/// <summary>
/// Handler for SetVoucherStatusCommand.
/// </summary>
public class SetVoucherStatusCommandHandler : IRequestHandler<SetVoucherStatusCommand, SetVoucherStatusResponse>
{
    private readonly IVoucherRepository _voucherRepository;

    public SetVoucherStatusCommandHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<SetVoucherStatusResponse> Handle(
        SetVoucherStatusCommand request,
        CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher == null)
        {
            return new SetVoucherStatusResponse(false, "Voucher not found.");
        }

        if (request.IsActive)
        {
            voucher.Activate();
        }
        else
        {
            voucher.Deactivate();
        }

        await _voucherRepository.UpdateAsync(voucher, cancellationToken);

        return new SetVoucherStatusResponse(true);
    }
}

/// <summary>
/// Handler for GetVoucherByIdQuery.
/// </summary>
public class GetVoucherByIdQueryHandler : IRequestHandler<GetVoucherByIdQuery, VoucherDetailDto?>
{
    private readonly IVoucherRepository _voucherRepository;

    public GetVoucherByIdQueryHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<VoucherDetailDto?> Handle(
        GetVoucherByIdQuery request,
        CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher == null) return null;

        return MapToDto(voucher);
    }

    private static VoucherDetailDto MapToDto(Voucher voucher)
    {
        return new VoucherDetailDto(
            voucher.Id,
            voucher.Code,
            voucher.Name,
            voucher.DiscountType,
            voucher.DiscountValue,
            voucher.MinOrderAmount.Amount,
            voucher.MaxDiscountAmount?.Amount,
            voucher.UsageLimit,
            voucher.UsageCount,
            voucher.RemainingUses,
            voucher.StartDate,
            voucher.EndDate,
            voucher.IsActive,
            voucher.IsValid(),
            voucher.SellerId
        );
    }
}
