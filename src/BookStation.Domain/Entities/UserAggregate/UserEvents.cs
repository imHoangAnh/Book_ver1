using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Base event for user-related domain events.
/// </summary>
public abstract class UserBaseEvent : DomainEvent
{
    public long UserId { get; }

    protected UserBaseEvent(long userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a new user is created.
/// </summary>
public sealed class UserCreatedEvent : UserBaseEvent
{
    public string Email { get; }

    public UserCreatedEvent(User user) : base(user.Id)
    {
        Email = user.Email.Value;
    }
}

/// <summary>
/// Event raised when a user is updated.
/// </summary>
public sealed class UserUpdatedEvent : UserBaseEvent
{
    public string UpdateType { get; }

    public UserUpdatedEvent(long userId, string updateType) : base(userId)
    {
        UpdateType = updateType;
    }
}

/// <summary>
/// Event raised when a user's email is changed.
/// </summary>
public sealed class UserEmailChangedEvent : UserBaseEvent
{
    public string OldEmail { get; }
    public string NewEmail { get; }

    public UserEmailChangedEvent(long userId, string oldEmail, string newEmail) : base(userId)
    {
        OldEmail = oldEmail;
        NewEmail = newEmail;
    }
}

/// <summary>
/// Event raised when a user's password is changed.
/// </summary>
public sealed class UserPasswordChangedEvent : UserBaseEvent
{
    public UserPasswordChangedEvent(long userId) : base(userId)
    {
    }
}

/// <summary>
/// Event raised when a user is verified.
/// </summary>
public sealed class UserVerifiedEvent : UserBaseEvent
{
    public UserVerifiedEvent(long userId) : base(userId)
    {
    }
}

/// <summary>
/// Event raised when a user is deactivated.
/// </summary>
public sealed class UserDeactivatedEvent : UserBaseEvent
{
    public UserDeactivatedEvent(long userId) : base(userId)
    {
    }
}

/// <summary>
/// Event raised when a user is banned.
/// </summary>
public sealed class UserBannedEvent : UserBaseEvent
{
    public string Reason { get; }

    public UserBannedEvent(long userId, string reason) : base(userId)
    {
        Reason = reason;
    }
}

/// <summary>
/// Event raised when a user becomes a seller.
/// </summary>
public sealed class UserBecameSellerEvent : UserBaseEvent
{
    public UserBecameSellerEvent(long userId) : base(userId)
    {
    }
}
