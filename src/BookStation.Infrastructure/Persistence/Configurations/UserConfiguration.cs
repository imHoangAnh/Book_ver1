using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(255);

        // Phone value object
        builder.OwnsOne(u => u.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
        });

        builder.Property(u => u.IsVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.SellerProfile)
            .WithOne(sp => sp.User)
            .HasForeignKey<SellerProfile>(sp => sp.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.ShipperProfile)
            .WithOne(sp => sp.User)
            .HasForeignKey<ShipperProfile>(sp => sp.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}
