namespace BookStation.Domain.Enums;

/// <summary>
/// Voucher discount type.
/// </summary>
public enum EDiscountType
{
    /// <summary>Percentage discount.</summary>
    Percent = 0,

    /// <summary>Fixed amount discount.</summary>
    Fixed = 1
}

/// <summary>
/// Organization type.
/// </summary>
public enum EOrganizationType
{
    /// <summary>Bookstore - retail store.</summary>
    Bookstore = 0,

    /// <summary>Publisher - publishing house selling directly.</summary>
    Publisher = 1
}
