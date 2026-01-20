using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IVoucherRepository _voucherRepository;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IVoucherRepository voucherRepository)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _voucherRepository = voucherRepository;
    }

    public async Task<CreateOrderResponse> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        // Create shipping address value object
        var shippingAddress = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.Country,
            request.ShippingAddress.Ward,
            request.ShippingAddress.District,
            request.ShippingAddress.PostalCode
        );

        // Create order
        var order = Order.Create(request.UserId, shippingAddress, request.Notes);

        // Add items to order
        foreach (var item in request.Items)
        {
            // Check inventory availability
            var inventory = await _inventoryRepository.GetByVariantIdAsync(item.VariantId, cancellationToken);
            if (inventory == null || !inventory.IsAvailable(item.Quantity))
            {
                throw new InvalidOperationException($"Insufficient stock for variant {item.VariantId}");
            }

            // Get current price
            var price = await _inventoryRepository.GetVariantPriceAsync(item.VariantId, cancellationToken);

            // Reserve inventory
            inventory.Reserve(item.Quantity, TimeSpan.FromMinutes(15));

            // Add item to order (need to get book title and variant name from DB)
            var unitPrice = Money.Create(price);
            order.AddItem(item.VariantId, item.Quantity, unitPrice, "Book Title", "Variant Name");
        }

        // Apply voucher if provided
        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            var voucher = await _voucherRepository.GetByCodeAsync(request.VoucherCode, cancellationToken);
            if (voucher != null && voucher.CanApply(order.TotalAmount))
            {
                var discount = voucher.CalculateDiscount(order.TotalAmount);
                order.ApplyVoucher(voucher.Id, discount);

                // Record voucher usage
                voucher.Use(request.UserId, order.Id);
            }
        }

        // Confirm order
        order.Confirm();

        // Save order
        await _orderRepository.AddAsync(order, cancellationToken);

        return new CreateOrderResponse(
            order.Id,
            order.TotalAmount.Amount,
            order.DiscountAmount.Amount,
            order.FinalAmount.Amount
        );
    }
}
