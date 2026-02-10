using BookStation.Domain.Entities.ReviewAggregate;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Reviews.Commands;

public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, long>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;

    public AddReviewCommandHandler(IReviewRepository reviewRepository, IOrderRepository orderRepository)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
    }

    public async Task<long> Handle(AddReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user has purchased the book
        // We use HasUserPurchasedBookAsync which verifies delivered orders containing the book
        var hasPurchased = await _orderRepository.HasUserPurchasedBookAsync(request.UserId, request.BookId, cancellationToken);
        if (!hasPurchased)
        {
            throw new InvalidOperationException("You can only review books you have purchased and received.");
        }

        // 2. Check if user already reviewed this book
        var existingReview = await _reviewRepository.GetByUserAndBookAsync(request.UserId, request.BookId, cancellationToken);
        if (existingReview != null)
        {
            throw new InvalidOperationException("You have already reviewed this book.");
        }

        // 3. Create Review
        var rating = Rating.Create(request.Rating);
        var review = Review.Create(
            request.BookId,
            request.UserId,
            rating,
            request.Content,
            request.HasSpoiler,
            request.IsAnonymous
        );

        await _reviewRepository.AddAsync(review, cancellationToken);
        
        // Assuming UnitOfWork pattern is used via Repository or DbContext, we need to save changes.
        // If Repository doesn't expose SaveChanges, we might need IUnitOfWork injection.
        // For this project structure, it seems Repositories usually handle it or IUnitOfWork is implied.
        // I'll call UnitOfWork.SaveChangesAsync if available in BaseRepository, or rely on common patterns here.
        // Looking at UserCommandHandler, it calls _userRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        
        if (_reviewRepository.UnitOfWork != null)
        {
            await _reviewRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return review.Id;
    }
}
