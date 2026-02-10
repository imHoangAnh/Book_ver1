using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BookStation.Application.Services;
using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Enums;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookStation.Infrastructure.Services;

/// <summary>
/// VNPay payment service implementation.
/// </summary>
public class VnPayService : IPaymentService
{
    private readonly VnPaySettings _settings;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<VnPayService> _logger;

    public VnPayService(
        IOptions<VnPaySettings> settings,
        IOrderRepository orderRepository,
        ILogger<VnPayService> logger)
    {
        _settings = settings.Value;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    /// <summary>
    /// Processes a Cash on Delivery payment.
    /// </summary>
    public async Task<PaymentResult> ProcessCashPaymentAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For COD, create a pending payment record
            var payment = order.AddPayment(order.FinalAmount, EPaymentMethod.Cash);
            
            // Generate a simple transaction reference
            var transactionId = $"COD_{order.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            _logger.LogInformation(
                "Cash payment created for order {OrderId}, Payment ID: {PaymentId}",
                order.Id, payment.Id);

            return new PaymentResult(
                Success: true,
                PaymentId: payment.Id,
                TransactionId: transactionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cash payment for order {OrderId}", order.Id);
            return new PaymentResult(
                Success: false,
                PaymentId: 0,
                TransactionId: null,
                ErrorMessage: ex.Message
            );
        }
    }

    /// <summary>
    /// Creates VNPay payment URL.
    /// </summary>
    public async Task<VnPayPaymentResult> CreateVnPayPaymentUrlAsync(
        Order order,
        string returnUrl,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create payment record
            var payment = order.AddPayment(order.FinalAmount, EPaymentMethod.VNPay);
            payment.StartProcessing();

            // Generate unique transaction reference
            var txnRef = $"{order.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Build VNPay payment URL
            var vnPayParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", _settings.Version },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _settings.TmnCode },
                { "vnp_Amount", ((long)(order.FinalAmount.Amount * 100)).ToString() },
                { "vnp_BankCode", "" },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", _settings.CurrencyCode },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", _settings.Locale },
                { "vnp_OrderInfo", $"Thanh toan don hang #{order.Id}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", txnRef },
                { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss") }
            };

            // Create query string
            var queryString = BuildQueryString(vnPayParams);
            
            // Create secure hash
            var secureHash = CreateSecureHash(queryString);
            
            var paymentUrl = $"{_settings.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";

            _logger.LogInformation(
                "VNPay payment URL created for order {OrderId}, TxnRef: {TxnRef}",
                order.Id, txnRef);

            return new VnPayPaymentResult(
                Success: true,
                PaymentUrl: paymentUrl,
                PaymentId: payment.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VNPay payment URL for order {OrderId}", order.Id);
            return new VnPayPaymentResult(
                Success: false,
                PaymentUrl: null,
                PaymentId: 0,
                ErrorMessage: ex.Message
            );
        }
    }

    /// <summary>
    /// Processes VNPay return callback.
    /// </summary>
    public async Task<PaymentResult> ProcessVnPayReturnAsync(
        VnPayReturnRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify secure hash
            if (!VerifySecureHash(request))
            {
                _logger.LogWarning("Invalid VNPay secure hash for TxnRef: {TxnRef}", request.VnpTxnRef);
                return new PaymentResult(
                    Success: false,
                    PaymentId: 0,
                    TransactionId: null,
                    ErrorMessage: "Invalid secure hash"
                );
            }

            // Extract order ID from transaction reference
            var orderIdStr = request.VnpTxnRef.Split('_')[0];
            if (!long.TryParse(orderIdStr, out var orderId))
            {
                return new PaymentResult(
                    Success: false,
                    PaymentId: 0,
                    TransactionId: null,
                    ErrorMessage: "Invalid transaction reference"
                );
            }

            // Get order
            var order = await _orderRepository.GetWithAllDetailsAsync(orderId, cancellationToken);
            if (order == null)
            {
                return new PaymentResult(
                    Success: false,
                    PaymentId: 0,
                    TransactionId: null,
                    ErrorMessage: "Order not found"
                );
            }

            // Find the processing VNPay payment
            var payment = order.Payments
                .FirstOrDefault(p => p.Method == EPaymentMethod.VNPay && p.Status == EPaymentStatus.Processing);

            if (payment == null)
            {
                return new PaymentResult(
                    Success: false,
                    PaymentId: 0,
                    TransactionId: null,
                    ErrorMessage: "Payment not found"
                );
            }

            // Check response code (00 = success)
            if (request.VnpResponseCode == "00" && request.VnpTransactionStatus == "00")
            {
                payment.Complete(request.VnpTransactionNo);
                
                // Confirm order if pending
                if (order.Status == EOrderStatus.Pending)
                {
                    order.Confirm();
                }

                await _orderRepository.UpdateAsync(order, cancellationToken);

                _logger.LogInformation(
                    "VNPay payment completed for order {OrderId}, Transaction: {TransactionNo}",
                    orderId, request.VnpTransactionNo);

                return new PaymentResult(
                    Success: true,
                    PaymentId: payment.Id,
                    TransactionId: request.VnpTransactionNo
                );
            }
            else
            {
                var errorMessage = GetVnPayErrorMessage(request.VnpResponseCode);
                payment.Fail(errorMessage);
                
                await _orderRepository.UpdateAsync(order, cancellationToken);

                _logger.LogWarning(
                    "VNPay payment failed for order {OrderId}, ResponseCode: {ResponseCode}",
                    orderId, request.VnpResponseCode);

                return new PaymentResult(
                    Success: false,
                    PaymentId: payment.Id,
                    TransactionId: null,
                    ErrorMessage: errorMessage
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay return for TxnRef: {TxnRef}", request.VnpTxnRef);
            return new PaymentResult(
                Success: false,
                PaymentId: 0,
                TransactionId: null,
                ErrorMessage: ex.Message
            );
        }
    }

    /// <summary>
    /// Gets payment status for an order.
    /// </summary>
    public async Task<PaymentStatusResult> GetPaymentStatusAsync(
        long orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithAllDetailsAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order {orderId} not found");

        var payments = order.Payments.Select(p => new PaymentInfo(
            p.Id,
            p.Amount.Amount,
            p.Method,
            p.Status,
            p.TransactionId,
            p.CompletedAt
        )).ToList();

        return new PaymentStatusResult(
            OrderId: orderId,
            Status: order.Payments.Any() 
                ? order.Payments.OrderByDescending(p => p.CreatedAt).First().Status 
                : EPaymentStatus.Pending,
            TotalAmount: order.FinalAmount.Amount,
            PaidAmount: order.TotalPaid.Amount,
            IsFullyPaid: order.IsFullyPaid,
            Payments: payments
        );
    }

    #region Private Helper Methods

    private string BuildQueryString(SortedDictionary<string, string> parameters)
    {
        var query = new StringBuilder();
        foreach (var kvp in parameters)
        {
            if (!string.IsNullOrEmpty(kvp.Value))
            {
                if (query.Length > 0)
                    query.Append('&');
                query.Append($"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}");
            }
        }
        return query.ToString();
    }

    private string CreateSecureHash(string queryString)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HashSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private bool VerifySecureHash(VnPayReturnRequest request)
    {
        var vnPayParams = new SortedDictionary<string, string>
        {
            { "vnp_Amount", request.VnpAmount },
            { "vnp_BankCode", request.VnpBankCode },
            { "vnp_BankTranNo", request.VnpBankTranNo ?? "" },
            { "vnp_CardType", request.VnpCardType ?? "" },
            { "vnp_OrderInfo", request.VnpOrderInfo },
            { "vnp_PayDate", request.VnpPayDate },
            { "vnp_ResponseCode", request.VnpResponseCode },
            { "vnp_TmnCode", request.VnpTmnCode },
            { "vnp_TransactionNo", request.VnpTransactionNo },
            { "vnp_TransactionStatus", request.VnpTransactionStatus },
            { "vnp_TxnRef", request.VnpTxnRef }
        };

        var queryString = BuildQueryString(vnPayParams);
        var expectedHash = CreateSecureHash(queryString);

        return expectedHash.Equals(request.VnpSecureHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetVnPayErrorMessage(string responseCode)
    {
        return responseCode switch
        {
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần.",
            "11" => "Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
            "12" => "Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
            "24" => "Khách hàng hủy giao dịch.",
            "51" => "Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
            "65" => "Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Khách hàng nhập sai mật khẩu thanh toán quá số lần quy định.",
            "99" => "Các lỗi khác.",
            _ => $"Lỗi không xác định: {responseCode}"
        };
    }

    #endregion
}
