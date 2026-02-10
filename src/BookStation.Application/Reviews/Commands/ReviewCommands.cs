using MediatR;

namespace BookStation.Application.Reviews.Commands;

public record AddReviewCommand(
    long BookId,
    long UserId, // Set from Controller
    int Rating,
    string? Content,
    bool HasSpoiler = false,
    bool IsAnonymous = false
) : IRequest<long>;
