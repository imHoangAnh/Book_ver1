using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.Entities.VoucherAggregate;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Query.Abstractions;

/// <summary>
/// Read-only database context for queries.
/// </summary>
public interface IReadDbContext
{
    DbSet<Book> Books { get; }
    DbSet<BookVariant> BookVariants { get; }
    DbSet<Author> Authors { get; }
    DbSet<Category> Categories { get; }
    DbSet<Publisher> Publishers { get; }
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Voucher> Vouchers { get; }
    DbSet<UserAddress> UserAddresses { get; }
}

