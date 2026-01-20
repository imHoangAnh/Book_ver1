namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Junction entity for User-Role many-to-many relationship.
/// </summary>
public class UserRole
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Gets the role ID.
    /// </summary>
    public int RoleId { get; private set; }

    /// <summary>
    /// Gets the assignment date.
    /// </summary>
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Role? Role { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private UserRole() { }

    /// <summary>
    /// Creates a new user-role association.
    /// </summary>
    internal static UserRole Create(long userId, int roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };
    }
}
