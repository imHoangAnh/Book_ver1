using BookStation.Core.SharedKernel;
namespace BookStation.Domain.Entities.UserAggregate;

/// <summary>
/// Permission entity for fine-grained authorization.
/// </summary>
public class Permission : Entity<int>
{
    /// <summary>
    /// Gets the permission name (e.g., "books.create", "orders.view").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the permission description.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the permission category (e.g., "Books", "Orders", "Users").
    /// </summary>
    public string Category { get; private set; } = null!;

    // Navigation properties
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Permission() { }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    public static Permission Create(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty.", nameof(name));

        return new Permission
        {
            Name = name.ToLowerInvariant().Trim(),
            Description = description.Trim(),
            Category = category.Trim()
        };
    }

    /// <summary>
    /// Predefined permissions.
    /// </summary>
    public static class Permissions
    {
        // Books
        public const string BooksView = "books.view";
        public const string BooksCreate = "books.create";
        public const string BooksUpdate = "books.update";
        public const string BooksDelete = "books.delete";

        // Orders
        public const string OrdersView = "orders.view";
        public const string OrdersCreate = "orders.create";
        public const string OrdersUpdate = "orders.update";
        public const string OrdersCancel = "orders.cancel";
        public const string OrdersViewAll = "orders.viewall"; // Admin only

        // Users
        public const string UsersView = "users.view";
        public const string UsersCreate = "users.create";
        public const string UsersUpdate = "users.update";
        public const string UsersDelete = "users.delete";

        // Inventory
        public const string InventoryView = "inventory.view";
        public const string InventoryUpdate = "inventory.update";

        // Vouchers
        public const string VouchersView = "vouchers.view";
        public const string VouchersCreate = "vouchers.create";
        public const string VouchersUpdate = "vouchers.update";
        public const string VouchersDelete = "vouchers.delete";
    }
}

/// <summary>
/// Junction entity for Role-Permission many-to-many relationship.
/// </summary>
public class RolePermission
{
    public long RoleId { get; private set; }
    public int PermissionId { get; private set; }

    public Role? Role { get; private set; }
    public Permission? Permission { get; private set; }

    private RolePermission() { }

    public static RolePermission Create(long roleId, int permissionId)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}
