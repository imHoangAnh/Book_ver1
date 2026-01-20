using MediatR;

namespace BookStation.Application.Orders.Commands;

/// <summary>
/// Command to create a new order.
/// </summary>
public record CreateOrderCommand(
    long UserId,
    List<OrderItemDto> Items,
    AddressDto ShippingAddress,
    string? VoucherCode = null,
    string? Notes = null
) : IRequest<CreateOrderResponse>;

public record OrderItemDto(
    long VariantId,
    int Quantity
);

public record AddressDto(
    string Street,
    string City,
    string Country,
    string? Ward = null,
    string? District = null,
    string? PostalCode = null
);

/// <summary>
/// Response for order creation.
/// </summary>
public record CreateOrderResponse(
    long OrderId,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount
);
