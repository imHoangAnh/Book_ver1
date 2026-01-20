using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.ReviewAggregate;

/// <summary>
/// Review entity for book reviews.
/// </summary>
public class Review : Entity<long>
{
    /// <summary>
    /// Gets the book ID.
    /// </summary>
    public long BookId { get; private set; }

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Gets the rating (1-5).
    /// </summary>
    public int Rating { get; private set; }

    /// <summary>
    /// Gets the review content.
    /// </summary>
    public string? Content { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the review contains spoilers.
    /// </summary>
    public bool HasSpoiler { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the review is anonymous.
    /// </summary>
    public bool IsAnonymous { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the review is approved.
    /// </summary>
    public bool IsApproved { get; private set; }

    /// <summary>
    /// Gets the number of helpful votes.
    /// </summary>
    public int HelpfulVotes { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Review() { }

    /// <summary>
    /// Creates a new review.
    /// </summary>
    public static Review Create(
        long bookId,
        long userId,
        int rating,
        string? content = null,
        bool hasSpoiler = false,
        bool isAnonymous = false)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        return new Review
        {
            BookId = bookId,
            UserId = userId,
            Rating = rating,
            Content = content?.Trim(),
            HasSpoiler = hasSpoiler,
            IsAnonymous = isAnonymous,
            IsApproved = true, // Auto-approve by default
            HelpfulVotes = 0
        };
    }

    /// <summary>
    /// Updates the review.
    /// </summary>
    public void Update(int rating, string? content, bool hasSpoiler)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        Rating = rating;
        Content = content?.Trim();
        HasSpoiler = hasSpoiler;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves the review.
    /// </summary>
    public void Approve()
    {
        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejects the review.
    /// </summary>
    public void Reject()
    {
        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increments the helpful vote count.
    /// </summary>
    public void AddHelpfulVote()
    {
        HelpfulVotes++;
        UpdatedAt = DateTime.UtcNow;
    }
}
