using BookStation.Domain.Entities.OrderAggregate;
using BookStation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        // Money value objects
        builder.OwnsOne(o => o.TotalAmount, ConfigureMoney("TotalAmount"));
        builder.OwnsOne(o => o.DiscountAmount, ConfigureMoney("DiscountAmount"));
        builder.OwnsOne(o => o.FinalAmount, ConfigureMoney("FinalAmount"));

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Address value object
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .HasMaxLength(500);

            address.Property(a => a.Ward)
                .HasColumnName("ShippingWard")
                .HasMaxLength(255);

            address.Property(a => a.District)
                .HasColumnName("ShippingDistrict")
                .HasMaxLength(255);

            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .HasMaxLength(255);

            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .HasMaxLength(255);

            address.Property(a => a.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .HasMaxLength(20);
        });

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(1000);

        // Relationships
        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(o => o.DomainEvents);

        // Index
        builder.HasIndex(o => o.UserId)
            .HasDatabaseName("IX_Orders_UserId");
    }

    private static Action<OwnedNavigationBuilder<Order, Money>> ConfigureMoney(string columnPrefix)
    {
        return money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName(columnPrefix)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName($"{columnPrefix}Currency")
                .HasMaxLength(3)
                .HasDefaultValue("VND");
        };
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.OwnsOne(oi => oi.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(oi => oi.Subtotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Subtotal")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("SubtotalCurrency")
                .HasMaxLength(3);
        });

        builder.Property(oi => oi.BookTitle)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(oi => oi.VariantName)
            .HasMaxLength(255)
            .IsRequired();
    }
}
