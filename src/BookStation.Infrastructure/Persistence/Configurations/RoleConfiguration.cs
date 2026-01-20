using BookStation.Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Role entity and RBAC relationships.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsSystemRole)
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        // Relationships
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for Permission entity.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Name");

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.Category)
            .HasDatabaseName("IX_Permissions_Category");
    }
}

/// <summary>
/// EF Core configuration for RolePermission junction table.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
