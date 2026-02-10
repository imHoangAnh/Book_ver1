namespace BookStation.Domain.Enums;

/// <summary>
/// Book publication status.
/// </summary>
public enum EBookStatus
{
    /// <summary>Draft - not yet published.</summary>
    Draft = 0,

    /// <summary>Active - available for sale.</summary>
    Active = 1,

    /// <summary>Inactive - not available.</summary>
    Inactive = 2,

    /// <summary>Out of stock.</summary>
    OutOfStock = 3, 

    /// <summary>Discontinued.</summary>
    Discontinued = 4
}

/// <summary>
/// Type of book bundle.
/// </summary>
public enum EBundleType
{
    /// <summary>Fixed set of books - customer buys the whole set.</summary>
    BundleSet = 0,

    /// <summary>Combo promotion - customer chooses books from eligible list.</summary>
    Combo = 1
}

