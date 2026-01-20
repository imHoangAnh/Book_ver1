using BookStation.Domain.Entities.CatalogAggregate;
using BookStation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStation.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.Title)
            .HasMaxLength(500)
            .IsRequired();

        // ISBN value object
        builder.OwnsOne(b => b.ISBN, isbn =>
        {
            isbn.Property(i => i.Value)
                .HasColumnName("ISBN")
                .HasMaxLength(20);

            isbn.HasIndex(i => i.Value)
                .IsUnique()
                .HasDatabaseName("IX_Books_ISBN");
        });

        builder.Property(b => b.Description)
            .HasMaxLength(5000);

        builder.Property(b => b.Language)
            .HasMaxLength(50);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.CoverImageUrl)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(b => b.Variants)
            .WithOne(v => v.Book)
            .HasForeignKey(v => v.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BookAuthors)
            .WithOne(ba => ba.Book)
            .HasForeignKey(ba => ba.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BookCategories)
            .WithOne(bc => bc.Book)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(b => b.DomainEvents);
    }
}

public class BookVariantConfiguration : IEntityTypeConfiguration<BookVariant>
{
    public void Configure(EntityTypeBuilder<BookVariant> builder)
    {
        builder.ToTable("BookVariants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VariantName)
            .HasMaxLength(255)
            .IsRequired();

        // Money value object for Price
        builder.OwnsOne(v => v.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            price.Property(p => p.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .HasDefaultValue("VND");
        });

        // Money value object for OriginalPrice
        builder.OwnsOne(v => v.OriginalPrice, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("OriginalPrice")
                .HasColumnType("decimal(18,2)");

            price.Property(p => p.Currency)
                .HasColumnName("OriginalPriceCurrency")
                .HasMaxLength(3);
        });

        builder.Property(v => v.SKU)
            .HasMaxLength(100);

        builder.HasOne(v => v.Inventory)
            .WithOne(i => i.Variant)
            .HasForeignKey<Inventory>(i => i.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
