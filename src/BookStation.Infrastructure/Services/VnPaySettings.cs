namespace BookStation.Infrastructure.Services;

/// <summary>
/// VNPay configuration settings.
/// </summary>
public class VnPaySettings
{
    public const string SectionName = "VnPay";

    /// <summary>
    /// Terminal ID provided by VNPay.
    /// </summary>
    public string TmnCode { get; set; } = string.Empty;

    /// <summary>
    /// Hash secret key for creating/verifying signatures.
    /// </summary>
    public string HashSecret { get; set; } = string.Empty;

    /// <summary>
    /// VNPay payment gateway URL.
    /// </summary>
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    /// <summary>
    /// API version.
    /// </summary>
    public string Version { get; set; } = "2.1.0";

    /// <summary>
    /// Currency code (VND).
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Locale (vn or en).
    /// </summary>
    public string Locale { get; set; } = "vn";

    /// <summary>
    /// Default return URL after payment.
    /// </summary>
    public string DefaultReturnUrl { get; set; } = string.Empty;
}
