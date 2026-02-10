using BookStation.Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for UserAddress entity.
/// </summary>
public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.Label)
            .HasMaxLength(50)
            .HasDefaultValue("Home");

        builder.Property(a => a.IsDefault)
            .HasDefaultValue(false);

        builder.Property(a => a.RecipientName)
            .HasMaxLength(100);

        builder.Property(a => a.RecipientPhone)
            .HasMaxLength(20);

        // Address value object (owned entity)
        builder.OwnsOne(a => a.Address, address =>
        {
            address.Property(addr => addr.Street)
                .HasColumnName("Street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(addr => addr.Ward)
                .HasColumnName("Ward")
                .HasMaxLength(100);

            address.Property(addr => addr.District)
                .HasColumnName("District")
                .HasMaxLength(100);

            address.Property(addr => addr.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(addr => addr.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(addr => addr.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20);
        });

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.UpdatedAt);

        // Relationships
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_UserAddresses_UserId");

        builder.HasIndex(a => new { a.UserId, a.IsDefault })
            .HasDatabaseName("IX_UserAddresses_UserId_IsDefault");
    }
}
