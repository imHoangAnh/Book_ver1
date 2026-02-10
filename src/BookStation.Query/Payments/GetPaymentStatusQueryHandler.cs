using BookStation.Query.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Query.Payments;

/// <summary>
/// Handler for GetPaymentStatusQuery.
/// </summary>
public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, PaymentStatusDto?>
{
    private readonly IReadDbContext _dbContext;

    public GetPaymentStatusQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentStatusDto?> Handle(
        GetPaymentStatusQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null) return null;

        var payments = order.Payments.Select(p => new PaymentDetailDto(
            p.Id,
            p.Amount.Amount,
            p.Method.ToString(),
            p.Status.ToString(),
            p.TransactionId,
            p.CreatedAt,
            p.CompletedAt
        )).ToList();

        return new PaymentStatusDto(
            order.Id,
            order.Status.ToString(),
            order.FinalAmount.Amount,
            order.TotalPaid.Amount,
            order.IsFullyPaid,
            payments
        );
    }
}

/// <summary>
/// Handler for GetOrderPaymentsQuery.
/// </summary>
public class GetOrderPaymentsQueryHandler : IRequestHandler<GetOrderPaymentsQuery, OrderPaymentsDto?>
{
    private readonly IReadDbContext _dbContext;

    public GetOrderPaymentsQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderPaymentsDto?> Handle(
        GetOrderPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null) return null;

        var payments = order.Payments.Select(p => new PaymentDetailDto(
            p.Id,
            p.Amount.Amount,
            p.Method.ToString(),
            p.Status.ToString(),
            p.TransactionId,
            p.CreatedAt,
            p.CompletedAt
        )).ToList();

        var paidAmount = order.TotalPaid.Amount;
        var remainingAmount = order.FinalAmount.Amount - paidAmount;

        return new OrderPaymentsDto(
            order.Id,
            order.TotalAmount.Amount,
            order.FinalAmount.Amount,
            paidAmount,
            remainingAmount > 0 ? remainingAmount : 0,
            payments
        );
    }
}
