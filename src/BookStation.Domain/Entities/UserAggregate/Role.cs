using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Role entity for RBAC (Role-Based Access Control).
/// Each role has a set of permissions.
/// </summary>
public class Role : Entity<long>
{
    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the role description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets whether this is a system role (cannot be deleted).
    /// </summary>
    public bool IsSystemRole { get; private set; }

    /// <summary>
    /// Gets whether this role is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    // RBAC: Role-Permissions relationship
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Role() { }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    public static Role Create(string name, string? description = null, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        return new Role
        {
            Name = name,
            Description = description,
            IsSystemRole = isSystemRole,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the role details.
    /// </summary>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Assigns a permission to this role.
    /// </summary>
    public void AddPermission(int permissionId)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            return; // Already has this permission

        _rolePermissions.Add(RolePermission.Create(Id, permissionId));
    }

    /// <summary>
    /// Removes a permission from this role.
    /// </summary>
    public void RemovePermission(int permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
    }

    /// <summary>
    /// Activates the role.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the role.
    /// </summary>
    public void Deactivate()
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot deactivate a system role.");

        IsActive = false;
    }

    /// <summary>
    /// Predefined system roles.
    /// </summary>
    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string Seller = "Seller";
        public const string User = "User";
        public const string Shipper = "Shipper";
        public const string Warehouse = "Warehouse";
    }
}
