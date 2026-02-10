using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.UserAggregate;

public sealed class UserSuspendedEvent : UserBaseEvent
{
    public string Reason { get; }
    public DateTime? SuspendUntil { get; }

    public UserSuspendedEvent(Guid userId, string reason, DateTime? suspendUntil) : base(userId)
    {
        Reason = reason;
        SuspendUntil = suspendUntil;
    }
}
