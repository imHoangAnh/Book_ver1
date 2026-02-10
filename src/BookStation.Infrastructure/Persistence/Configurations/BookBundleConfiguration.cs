using BookStation.Domain.Entities.CatalogAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

public class BookBundleConfiguration : IEntityTypeConfiguration<BookBundle>
{
    public void Configure(EntityTypeBuilder<BookBundle> builder)
    {
        builder.ToTable("BookBundles");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.CoverImageUrl)
            .HasMaxLength(1000);

        builder.Property(b => b.BundleType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Money value object for Price (BundleSet only)
        builder.OwnsOne(b => b.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)");

            price.Property(p => p.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .HasDefaultValue("VND");
        });

        // Discount settings (Combo only)
        builder.Property(b => b.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.DiscountValue)
            .HasColumnType("decimal(18,2)");

        // Index for seller lookup
        builder.HasIndex(b => b.SellerId)
            .HasDatabaseName("IX_BookBundles_SellerId");

        // Index for active bundles
        builder.HasIndex(b => new { b.SellerId, b.IsActive })
            .HasDatabaseName("IX_BookBundles_SellerId_IsActive");

        // Unique name per seller
        builder.HasIndex(b => new { b.SellerId, b.Name })
            .IsUnique()
            .HasDatabaseName("IX_BookBundles_SellerId_Name");

        // Relationships
        builder.HasMany(b => b.Items)
            .WithOne(i => i.Bundle)
            .HasForeignKey(i => i.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(b => b.DomainEvents);
    }
}

public class BookBundleItemConfiguration : IEntityTypeConfiguration<BookBundleItem>
{
    public void Configure(EntityTypeBuilder<BookBundleItem> builder)
    {
        builder.ToTable("BookBundleItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        // Index for bundle lookup
        builder.HasIndex(i => i.BundleId)
            .HasDatabaseName("IX_BookBundleItems_BundleId");

        // Index for book lookup
        builder.HasIndex(i => i.BookId)
            .HasDatabaseName("IX_BookBundleItems_BookId");

        // Unique constraint: same book can only appear once per bundle
        builder.HasIndex(i => new { i.BundleId, i.BookId, i.VariantId })
            .IsUnique()
            .HasDatabaseName("IX_BookBundleItems_Bundle_Book_Variant");

        // Relationships
        builder.HasOne(i => i.Book)
            .WithMany()
            .HasForeignKey(i => i.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Variant)
            .WithMany()
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
