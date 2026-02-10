using BookStation.Application.Services;
using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Enums;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Orders.Commands;

/// <summary>
/// Handler for CheckoutCommand.
/// </summary>
public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IPaymentService _paymentService;

    public CheckoutCommandHandler(
        IOrderRepository orderRepository,
        IUserAddressRepository userAddressRepository,
        IInventoryRepository inventoryRepository,
        IVoucherRepository voucherRepository,
        IPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _userAddressRepository = userAddressRepository;
        _inventoryRepository = inventoryRepository;
        _voucherRepository = voucherRepository;
        _paymentService = paymentService;
    }

    public async Task<CheckoutResponse> Handle(
        CheckoutCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get shipping address
            var shippingAddress = await GetShippingAddressAsync(
                request.UserId, request.ShippingAddressId, cancellationToken);

            if (shippingAddress == null)
            {
                return new CheckoutResponse(
                    false, null, 0, 0, 0,
                    request.PaymentMethod, EPaymentStatus.Failed,
                    ErrorMessage: "No shipping address found. Please add an address first.");
            }

            // 2. Create order
            var order = Order.Create(
                GetUserIdAsLong(request.UserId),
                shippingAddress,
                request.Notes
            );

            // 3. Add items to order
            foreach (var item in request.Items)
            {
                var inventory = await _inventoryRepository.GetByVariantIdAsync(item.VariantId, cancellationToken);
                if (inventory == null || !inventory.IsAvailable(item.Quantity))
                {
                    return new CheckoutResponse(
                        false, null, 0, 0, 0,
                        request.PaymentMethod, EPaymentStatus.Failed,
                        ErrorMessage: $"Insufficient stock for variant {item.VariantId}");
                }

                var price = await _inventoryRepository.GetVariantPriceAsync(item.VariantId, cancellationToken);
                var variantInfo = await _inventoryRepository.GetVariantInfoAsync(item.VariantId, cancellationToken);
                
                inventory.Reserve(item.Quantity, TimeSpan.FromMinutes(30));
                
                var unitPrice = Money.Create(price);
                order.AddItem(item.VariantId, item.Quantity, unitPrice, 
                    variantInfo?.BookTitle ?? "Unknown", 
                    variantInfo?.VariantName ?? "Default");
            }

            // 4. Apply voucher if provided
            if (!string.IsNullOrWhiteSpace(request.VoucherCode))
            {
                var voucher = await _voucherRepository.GetByCodeAsync(request.VoucherCode, cancellationToken);
                if (voucher != null && voucher.CanApply(order.TotalAmount) && !voucher.HasUserUsed(GetUserIdAsLong(request.UserId)))
                {
                    var discount = voucher.CalculateDiscount(order.TotalAmount);
                    order.ApplyVoucher(voucher.Id, discount);
                    voucher.Use(GetUserIdAsLong(request.UserId), order.Id);
                }
            }

            // 5. Save order first to get ID
            await _orderRepository.AddAsync(order, cancellationToken);

            // 6. Process payment based on method
            string? paymentUrl = null;
            var paymentStatus = EPaymentStatus.Pending;

            switch (request.PaymentMethod)
            {
                case EPaymentMethod.Cash:
                    var cashResult = await _paymentService.ProcessCashPaymentAsync(order, cancellationToken);
                    if (cashResult.Success)
                    {
                        order.Confirm();
                        paymentStatus = EPaymentStatus.Pending; // Will be completed on delivery
                    }
                    break;

                case EPaymentMethod.VNPay:
                    if (string.IsNullOrEmpty(request.ReturnUrl))
                    {
                        return new CheckoutResponse(
                            false, order.Id, order.TotalAmount.Amount, order.DiscountAmount.Amount, order.FinalAmount.Amount,
                            request.PaymentMethod, EPaymentStatus.Failed,
                            ErrorMessage: "Return URL is required for VNPay payment.");
                    }

                    var vnPayResult = await _paymentService.CreateVnPayPaymentUrlAsync(
                        order, request.ReturnUrl, request.IpAddress ?? "127.0.0.1", cancellationToken);
                    
                    if (vnPayResult.Success)
                    {
                        paymentUrl = vnPayResult.PaymentUrl;
                        paymentStatus = EPaymentStatus.Processing;
                    }
                    else
                    {
                        return new CheckoutResponse(
                            false, order.Id, order.TotalAmount.Amount, order.DiscountAmount.Amount, order.FinalAmount.Amount,
                            request.PaymentMethod, EPaymentStatus.Failed,
                            ErrorMessage: vnPayResult.ErrorMessage);
                    }
                    break;

                default:
                    return new CheckoutResponse(
                        false, order.Id, order.TotalAmount.Amount, order.DiscountAmount.Amount, order.FinalAmount.Amount,
                        request.PaymentMethod, EPaymentStatus.Failed,
                        ErrorMessage: $"Payment method {request.PaymentMethod} is not supported.");
            }

            // 7. Update order
            await _orderRepository.UpdateAsync(order, cancellationToken);

            return new CheckoutResponse(
                Success: true,
                OrderId: order.Id,
                TotalAmount: order.TotalAmount.Amount,
                DiscountAmount: order.DiscountAmount.Amount,
                FinalAmount: order.FinalAmount.Amount,
                PaymentMethod: request.PaymentMethod,
                PaymentStatus: paymentStatus,
                PaymentUrl: paymentUrl
            );
        }
        catch (Exception ex)
        {
            return new CheckoutResponse(
                false, null, 0, 0, 0,
                request.PaymentMethod, EPaymentStatus.Failed,
                ErrorMessage: ex.Message);
        }
    }

    private async Task<Address?> GetShippingAddressAsync(
        Guid userId,
        int? addressId,
        CancellationToken cancellationToken)
    {
        Domain.Entities.UserAggregate.UserAddress? userAddress;

        if (addressId.HasValue)
        {
            // Use specified address
            userAddress = await _userAddressRepository.GetByIdAsync(addressId.Value, cancellationToken);
            if (userAddress == null || userAddress.UserId != userId)
            {
                return null;
            }
        }
        else
        {
            // Use default address
            userAddress = await _userAddressRepository.GetDefaultAddressAsync(userId, cancellationToken);
        }

        return userAddress?.Address;
    }

    private static long GetUserIdAsLong(Guid userId)
    {
        // Convert Guid to long for order's UserId
        // This is a simple hash - in production you might want to use a mapping table
        return Math.Abs(userId.GetHashCode());
    }
}

/// <summary>
/// Handler for ValidateVoucherCommand.
/// </summary>
public class ValidateVoucherCommandHandler : IRequestHandler<ValidateVoucherCommand, ValidateVoucherResponse>
{
    private readonly IVoucherRepository _voucherRepository;

    public ValidateVoucherCommandHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<ValidateVoucherResponse> Handle(
        ValidateVoucherCommand request,
        CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByCodeAsync(request.VoucherCode, cancellationToken);
        
        if (voucher == null)
        {
            return new ValidateVoucherResponse(
                false, null, null, 0, "Voucher code not found.");
        }

        if (!voucher.IsValid())
        {
            return new ValidateVoucherResponse(
                false, voucher.Id, voucher.Name, 0, "Voucher is expired or inactive.");
        }

        var orderAmount = Money.Create(request.OrderAmount);
        if (!voucher.CanApply(orderAmount))
        {
            return new ValidateVoucherResponse(
                false, voucher.Id, voucher.Name, 0, 
                $"Minimum order amount is {voucher.MinOrderAmount.Amount:N0} VND.");
        }

        var userId = Math.Abs(request.UserId.GetHashCode());
        if (voucher.HasUserUsed(userId))
        {
            return new ValidateVoucherResponse(
                false, voucher.Id, voucher.Name, 0, "You have already used this voucher.");
        }

        var discount = voucher.CalculateDiscount(orderAmount);
        
        return new ValidateVoucherResponse(
            true, voucher.Id, voucher.Name, discount.Amount);
    }
}

/// <summary>
/// Handler for GetAvailableVouchersQuery.
/// </summary>
public class GetAvailableVouchersQueryHandler : IRequestHandler<GetAvailableVouchersQuery, AvailableVouchersResponse>
{
    private readonly IVoucherRepository _voucherRepository;

    public GetAvailableVouchersQueryHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<AvailableVouchersResponse> Handle(
        GetAvailableVouchersQuery request,
        CancellationToken cancellationToken)
    {
        var orderAmount = Money.Create(request.OrderAmount);
        var userId = Math.Abs(request.UserId.GetHashCode());
        
        var vouchers = await _voucherRepository.GetAvailableVouchersAsync(
            orderAmount, userId, cancellationToken);

        var voucherDtos = vouchers.Select(v => new VoucherDto(
            v.Id,
            v.Code,
            v.Name,
            v.DiscountType,
            v.DiscountValue,
            v.MinOrderAmount.Amount,
            v.MaxDiscountAmount?.Amount,
            v.StartDate,
            v.EndDate,
            v.CalculateDiscount(orderAmount).Amount
        )).ToList();

        return new AvailableVouchersResponse(voucherDtos);
    }
}
