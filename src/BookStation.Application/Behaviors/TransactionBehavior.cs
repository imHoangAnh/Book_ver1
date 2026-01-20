using BookStation.Core.SharedKernel;
using MediatR;

namespace BookStation.Application.Behaviors;

/// <summary>
/// Pipeline behavior for wrapping commands in a database transaction.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply transaction for commands (not queries)
        var requestName = typeof(TRequest).Name;
        if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return await next();
        }

        // Execute command within transaction
        var response = await next();

        // Save changes (this will also publish domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
