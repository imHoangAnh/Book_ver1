using BookStation.Application.Reviews.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStation.PublicApi.Controllers;

/// <summary>
/// Controller for managing product reviews.
/// </summary>
[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Submit a review for a book.
    /// User must have purchased and received the book.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        // Convert Guid to long for internal ID if needed, 
        // BUT wait, User entity uses Guid as Id in User.cs: public class User : AggregateRoot<Guid>
        // BUT Review.cs: public long UserId { get; private set; }
        // AND validation in CheckoutCommandHandler uses: Math.Abs(userId.GetHashCode()) to convert Guid to Long.
        // This is a known inconsistency in this codebase (User is Guid, other entities use Long for UserId).
        // I must follow the existing pattern: Math.Abs(userGuid.GetHashCode()).
        
        long userLongId = Math.Abs(userGuid.GetHashCode());

        var command = new AddReviewCommand(
            request.BookId,
            userLongId,
            request.Rating,
            request.Content,
            request.HasSpoiler,
            request.IsAnonymous
        );

        try
        {
            var reviewId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = reviewId }, new { id = reviewId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a review by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        // Placeholder for GetById query
        return Ok(new { message = "Get Review working", id });
    }
}

public record CreateReviewRequest(
    long BookId,
    int Rating,
    string? Content,
    bool HasSpoiler = false,
    bool IsAnonymous = false
);
