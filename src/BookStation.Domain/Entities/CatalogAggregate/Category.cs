using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.CatalogAggregate;

/// <summary>
/// Category entity for book classification.
/// </summary>
public class Category : Entity<int>
{
    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the category description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the parent category ID (for hierarchical categories).
    /// </summary>
    public int? ParentId { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the category is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the slug for SEO-friendly URLs.
    /// </summary>
    public string? Slug { get; private set; }

    // Navigation properties
    public Category? Parent { get; private set; }

    private readonly List<Category> _children = [];
    public IReadOnlyList<Category> Children => _children.AsReadOnly();

    private readonly List<BookCategory> _bookCategories = [];
    public IReadOnlyList<BookCategory> BookCategories => _bookCategories.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Category() { }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    public static Category Create(string name, string? description = null, int? parentId = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new Category
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            ParentId = parentId,
            DisplayOrder = displayOrder,
            IsActive = true,
            Slug = GenerateSlug(name)
        };
    }

    /// <summary>
    /// Updates the category.
    /// </summary>
    public void Update(string name, string? description, int? parentId, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        ParentId = parentId;
        DisplayOrder = displayOrder;
        Slug = GenerateSlug(name);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the category.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the category.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the number of books in this category.
    /// </summary>
    public int BookCount => _bookCategories.Count;

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
    }
}
