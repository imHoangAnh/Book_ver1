namespace BookStation.Domain.Enums;

/// <summary>
/// User account status.
/// </summary>
public enum EUserStatus
{
    /// <summary>Active user account.</summary>
    Active = 0,

    /// <summary>Inactive user account.</summary>
    Inactive = 1,

    /// <summary>Banned user account.</summary>
    Banned = 2,

    /// <summary>Suspended user account.</summary>
    Suspended = 3,

    /// <summary>Pending verification.</summary>
    Pending = 4
}

/// <summary>
/// Gender options.
/// </summary>
public enum EGender
{
    /// <summary>Female.</summary>
    Female = 0,

    /// <summary>Male.</summary>
    Male = 1,

    /// <summary>Other.</summary>
    Other = 2
}
