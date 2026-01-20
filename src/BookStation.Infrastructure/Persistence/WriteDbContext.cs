using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.Entities.CartAggregate;
using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Entities.VoucherAggregate;
using BookStation.Domain.Entities.ShipmentAggregate;
using BookStation.Domain.Entities.OrganizationAggregate;
using BookStation.Domain.Entities.ReviewAggregate;
using BookStation.Core.SharedKernel;
using BookStation.Query.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Infrastructure.Persistence;

/// <summary>
/// Main database context for write operations.
/// </summary>
public class WriteDbContext : DbContext, IUnitOfWork, IReadDbContext
{
    private readonly IPublisher _publisher;

    public WriteDbContext(DbContextOptions<WriteDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    // User Aggregate
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<ShipperProfile> ShipperProfiles => Set<ShipperProfile>();

    // RBAC
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Catalog Aggregate
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookVariant> BookVariants => Set<BookVariant>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();

    // Order Aggregate
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Refund> Refunds => Set<Refund>();

    // Cart Aggregate
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // Voucher Aggregate
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<UserVoucherUsage> UserVoucherUsages => Set<UserVoucherUsage>();

    // Shipment Aggregate
    public DbSet<Shipment> Shipments => Set<Shipment>();

    // Organization
    public DbSet<Organization> Organizations => Set<Organization>();

    // Review & Social
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publish domain events before saving
        await PublishDomainEventsAsync(cancellationToken);

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        var aggregateRoots = ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        // Clear domain events
        aggregateRoots.ForEach(ar => ar.ClearDomainEvents());

        // Publish domain events
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
