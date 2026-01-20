using BookStation.Domain.Entities.UserAggregate;
using BookStation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookStation.Infrastructure.Data;

/// <summary>
/// Seeds initial data for RBAC (Roles and Permissions).
/// Run once during application startup.
/// </summary>
public class RbacDataSeeder
{
    private readonly WriteDbContext _context;

    public RbacDataSeeder(WriteDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Seed Permissions first
        await SeedPermissionsAsync();

        // Seed Roles
        await SeedRolesAsync();

        // Assign Permissions to Roles
        await AssignPermissionsToRolesAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        if (await _context.Permissions.AnyAsync())
            return; // Already seeded

        var permissions = new List<Permission>
        {
            // Books
            Permission.Create("books.view", "View books", "Books"),
            Permission.Create("books.create", "Create books", "Books"),
            Permission.Create("books.update", "Update books", "Books"),
            Permission.Create("books.delete", "Delete books", "Books"),

            // Orders
            Permission.Create("orders.view", "View own orders", "Orders"),
            Permission.Create("orders.create", "Create orders", "Orders"),
            Permission.Create("orders.update", "Update own orders", "Orders"),
            Permission.Create("orders.cancel", "Cancel own orders", "Orders"),
            Permission.Create("orders.viewall", "View all orders", "Orders"),
            Permission.Create("orders.manage", "Manage all orders", "Orders"),

            // Users
            Permission.Create("users.view", "View users", "Users"),
            Permission.Create("users.create", "Create users", "Users"),
            Permission.Create("users.update", "Update users", "Users"),
            Permission.Create("users.delete", "Delete users", "Users"),

            // Inventory
            Permission.Create("inventory.view", "View inventory", "Inventory"),
            Permission.Create("inventory.update", "Update inventory", "Inventory"),

            // Vouchers
            Permission.Create("vouchers.view", "View vouchers", "Vouchers"),
            Permission.Create("vouchers.create", "Create vouchers", "Vouchers"),
            Permission.Create("vouchers.update", "Update vouchers", "Vouchers"),
            Permission.Create("vouchers.delete", "Delete vouchers", "Vouchers"),

            // Shipments
            Permission.Create("shipments.view", "View shipments", "Shipments"),
            Permission.Create("shipments.update", "Update shipments", "Shipments"),

            // Reports
            Permission.Create("reports.sales", "View sales reports", "Reports"),
            Permission.Create("reports.inventory", "View inventory reports", "Reports"),
        };

        await _context.Permissions.AddRangeAsync(permissions);
    }

    private async Task SeedRolesAsync()
    {
        if (await _context.Roles.AnyAsync())
            return; // Already seeded

        var roles = new List<Role>
        {
            Role.Create(Role.SystemRoles.Admin, "Administrator with full access", isSystemRole: true),
            Role.Create(Role.SystemRoles.User, "Regular user", isSystemRole: true),
            Role.Create(Role.SystemRoles.Seller, "Seller who can manage books and orders", isSystemRole: true),
            Role.Create(Role.SystemRoles.Shipper, "Shipper who can manage deliveries", isSystemRole: true),
            Role.Create(Role.SystemRoles.Warehouse, "Warehouse staff who can manage inventory", isSystemRole: true),
        };

        await _context.Roles.AddRangeAsync(roles);
    }

    private async Task AssignPermissionsToRolesAsync()
    {
        var roles = await _context.Roles.Include(r => r.RolePermissions).ToListAsync();
        var permissions = await _context.Permissions.ToListAsync();

        var admin = roles.First(r => r.Name == Role.SystemRoles.Admin);
        var user = roles.First(r => r.Name == Role.SystemRoles.User);
        var seller = roles.First(r => r.Name == Role.SystemRoles.Seller);
        var shipper = roles.First(r => r.Name == Role.SystemRoles.Shipper);
        var warehouse = roles.First(r => r.Name == Role.SystemRoles.Warehouse);

        // Admin: ALL permissions
        foreach (var permission in permissions)
        {
            admin.AddPermission(permission.Id);
        }

        // User: Basic permissions
        user.AddPermission(GetPermissionId(permissions, "books.view"));
        user.AddPermission(GetPermissionId(permissions, "orders.view"));
        user.AddPermission(GetPermissionId(permissions, "orders.create"));
        user.AddPermission(GetPermissionId(permissions, "orders.cancel"));

        // Seller: Books + Orders + Inventory
        seller.AddPermission(GetPermissionId(permissions, "books.view"));
        seller.AddPermission(GetPermissionId(permissions, "books.create"));
        seller.AddPermission(GetPermissionId(permissions, "books.update"));
        seller.AddPermission(GetPermissionId(permissions, "orders.viewall"));
        seller.AddPermission(GetPermissionId(permissions, "orders.manage"));
        seller.AddPermission(GetPermissionId(permissions, "inventory.view"));
        seller.AddPermission(GetPermissionId(permissions, "inventory.update"));
        seller.AddPermission(GetPermissionId(permissions, "reports.sales"));

        // Shipper: Shipments
        shipper.AddPermission(GetPermissionId(permissions, "shipments.view"));
        shipper.AddPermission(GetPermissionId(permissions, "shipments.update"));
        shipper.AddPermission(GetPermissionId(permissions, "orders.viewall"));

        // Warehouse: Inventory
        warehouse.AddPermission(GetPermissionId(permissions, "inventory.view"));
        warehouse.AddPermission(GetPermissionId(permissions, "inventory.update"));
        warehouse.AddPermission(GetPermissionId(permissions, "books.view"));
        warehouse.AddPermission(GetPermissionId(permissions, "reports.inventory"));
    }

    private static int GetPermissionId(List<Permission> permissions, string permissionName)
    {
        return permissions.First(p => p.Name == permissionName).Id;
    }
}

/// <summary>
/// Extension to run seeder on startup.
/// </summary>
public static class RbacDataSeederExtensions
{
    public static async Task SeedRbacDataAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var seeder = new RbacDataSeeder(context);
        await seeder.SeedAsync();
    }
}
