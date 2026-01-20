using MediatR;

namespace BookStation.Query.Orders;

/// <summary>
/// Query to get order details by ID.
/// </summary>
public record GetOrderByIdQuery(long OrderId) : IRequest<OrderDetailDto?>;

/// <summary>
/// Order detail DTO.
/// </summary>
public record OrderDetailDto(
    long Id,
    long UserId,
    string Status,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    string ShippingAddress,
    string? Notes,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    long Id,
    string BookTitle,
    string VariantName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);
