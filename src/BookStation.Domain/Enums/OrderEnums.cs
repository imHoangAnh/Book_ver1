namespace BookStation.Domain.Enums;

/// <summary>
/// Order status.
/// </summary>
public enum EOrderStatus
{
    /// <summary>Order is pending confirmation.</summary>
    Pending = 0,

    /// <summary>Order has been confirmed.</summary>
    Confirmed = 1,

    /// <summary>Order is being processed.</summary>
    Processing = 2,

    /// <summary>Order has been shipped.</summary>
    Shipped = 3,

    /// <summary>Order has been delivered.</summary>
    Delivered = 4,

    /// <summary>Order has been cancelled.</summary>
    Cancelled = 5,

    /// <summary>Order has been returned.</summary>
    Returned = 6,

    /// <summary>Order has been refunded.</summary>
    Refunded = 7
}

/// <summary>
/// Payment method.
/// </summary>
public enum EPaymentMethod
{
    /// <summary>Cash on delivery.</summary>
    Cash = 0,

    /// <summary>Credit/Debit card.</summary>
    CreditCard = 1,

    /// <summary>Bank transfer.</summary>
    BankTransfer = 2,

    /// <summary>E-wallet (Momo, ZaloPay, etc.).</summary>
    EWallet = 3,

    /// <summary>VNPAY.</summary>
    VNPay = 4
}

/// <summary>
/// Payment status.
/// </summary>
public enum EPaymentStatus
{
    /// <summary>Payment is pending.</summary>
    Pending = 0,

    /// <summary>Payment is being processed.</summary>
    Processing = 1,

    /// <summary>Payment completed successfully.</summary>
    Completed = 2,

    /// <summary>Payment failed.</summary>
    Failed = 3,

    /// <summary>Payment was cancelled.</summary>
    Cancelled = 4,

    /// <summary>Payment was refunded.</summary>
    Refunded = 5
}
