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
/// Book author role in the book.
/// </summary>
public enum EAuthorRole
{
    /// <summary>Main author.</summary>
    Author = 0,

    /// <summary>Editor.</summary>
    Editor = 1,

    /// <summary>Illustrator.</summary>
    Illustrator = 2,

    /// <summary>Translator.</summary>
    Translator = 3,

    /// <summary>Co-author.</summary>
    CoAuthor = 4
}
