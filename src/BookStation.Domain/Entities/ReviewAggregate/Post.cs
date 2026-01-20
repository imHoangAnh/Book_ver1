using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.ReviewAggregate;

/// <summary>
/// Post entity for social posts.
/// </summary>
public class Post : Entity<long>
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Gets the post content.
    /// </summary>
    public string Content { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the post is published.
    /// </summary>
    public bool IsPublished { get; private set; }

    /// <summary>
    /// Gets the like count.
    /// </summary>
    public int LikeCount { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Post() { }

    /// <summary>
    /// Creates a new post.
    /// </summary>
    public static Post Create(long userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        return new Post
        {
            UserId = userId,
            Content = content.Trim(),
            IsPublished = true,
            LikeCount = 0
        };
    }

    /// <summary>
    /// Updates the post content.
    /// </summary>
    public void Update(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Publishes the post.
    /// </summary>
    public void Publish()
    {
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unpublishes the post.
    /// </summary>
    public void Unpublish()
    {
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increments the like count.
    /// </summary>
    public void AddLike()
    {
        LikeCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrements the like count.
    /// </summary>
    public void RemoveLike()
    {
        if (LikeCount > 0)
        {
            LikeCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
