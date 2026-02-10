using BookStation.Application.Services;
using BookStation.Domain.Enums;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Payments.Commands;

/// <summary>
/// Handler for ProcessPaymentCommand.
/// </summary>
public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;

    public ProcessPaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
    }

    public async Task<ProcessPaymentResponse> Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // Get order with payment details
        var order = await _orderRepository.GetWithAllDetailsAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return new ProcessPaymentResponse(
                false, 0, request.PaymentMethod, EPaymentStatus.Failed,
                ErrorMessage: "Order not found.");
        }

        // Check if order can be paid
        if (order.Status == EOrderStatus.Cancelled)
        {
            return new ProcessPaymentResponse(
                false, 0, request.PaymentMethod, EPaymentStatus.Failed,
                ErrorMessage: "Cannot pay for a cancelled order.");
        }

        if (order.IsFullyPaid)
        {
            return new ProcessPaymentResponse(
                false, 0, request.PaymentMethod, EPaymentStatus.Failed,
                ErrorMessage: "Order is already fully paid.");
        }

        // Process payment based on method
        switch (request.PaymentMethod)
        {
            case EPaymentMethod.Cash:
                return await ProcessCashPaymentAsync(order, cancellationToken);

            case EPaymentMethod.VNPay:
                if (string.IsNullOrEmpty(request.ReturnUrl))
                {
                    return new ProcessPaymentResponse(
                        false, 0, EPaymentMethod.VNPay, EPaymentStatus.Failed,
                        ErrorMessage: "Return URL is required for VNPay payment.");
                }
                return await ProcessVnPayPaymentAsync(order, request.ReturnUrl, request.IpAddress ?? "127.0.0.1", cancellationToken);

            default:
                return new ProcessPaymentResponse(
                    false, 0, request.PaymentMethod, EPaymentStatus.Failed,
                    ErrorMessage: $"Payment method {request.PaymentMethod} is not supported.");
        }
    }

    private async Task<ProcessPaymentResponse> ProcessCashPaymentAsync(
        Domain.Entities.OrderAggregate.Order order,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.ProcessCashPaymentAsync(order, cancellationToken);

        if (result.Success)
        {
            // Confirm the order for COD
            if (order.Status == EOrderStatus.Pending)
            {
                order.Confirm();
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            return new ProcessPaymentResponse(
                true,
                result.PaymentId,
                EPaymentMethod.Cash,
                EPaymentStatus.Pending,  // COD is pending until delivery
                TransactionId: result.TransactionId
            );
        }

        return new ProcessPaymentResponse(
            false, 0, EPaymentMethod.Cash, EPaymentStatus.Failed,
            ErrorMessage: result.ErrorMessage
        );
    }

    private async Task<ProcessPaymentResponse> ProcessVnPayPaymentAsync(
        Domain.Entities.OrderAggregate.Order order,
        string returnUrl,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.CreateVnPayPaymentUrlAsync(
            order, returnUrl, ipAddress, cancellationToken);

        if (result.Success)
        {
            await _orderRepository.UpdateAsync(order, cancellationToken);

            return new ProcessPaymentResponse(
                true,
                result.PaymentId,
                EPaymentMethod.VNPay,
                EPaymentStatus.Processing,
                PaymentUrl: result.PaymentUrl
            );
        }

        return new ProcessPaymentResponse(
            false, 0, EPaymentMethod.VNPay, EPaymentStatus.Failed,
            ErrorMessage: result.ErrorMessage
        );
    }
}

/// <summary>
/// Handler for ConfirmVnPayPaymentCommand.
/// </summary>
public class ConfirmVnPayPaymentCommandHandler : IRequestHandler<ConfirmVnPayPaymentCommand, ConfirmVnPayPaymentResponse>
{
    private readonly IPaymentService _paymentService;

    public ConfirmVnPayPaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<ConfirmVnPayPaymentResponse> Handle(
        ConfirmVnPayPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var vnPayRequest = new VnPayReturnRequest(
            request.VnpTxnRef,
            request.VnpAmount,
            request.VnpBankCode,
            request.VnpBankTranNo,
            request.VnpCardType,
            request.VnpOrderInfo,
            request.VnpPayDate,
            request.VnpResponseCode,
            request.VnpTmnCode,
            request.VnpTransactionNo,
            request.VnpTransactionStatus,
            request.VnpSecureHash
        );

        var result = await _paymentService.ProcessVnPayReturnAsync(vnPayRequest, cancellationToken);

        if (result.Success)
        {
            // Extract order ID from transaction reference
            var orderId = long.Parse(request.VnpTxnRef.Split('_')[0]);

            return new ConfirmVnPayPaymentResponse(
                true,
                orderId,
                result.PaymentId,
                result.TransactionId
            );
        }

        return new ConfirmVnPayPaymentResponse(
            false, 0, 0, null,
            ErrorMessage: result.ErrorMessage
        );
    }
}
