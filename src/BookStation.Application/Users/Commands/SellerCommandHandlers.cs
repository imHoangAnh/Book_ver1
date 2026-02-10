using BookStation.Domain.Repositories;
using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Handler for ApproveSellerCommand.
/// </summary>
public class ApproveSellerCommandHandler : IRequestHandler<ApproveSellerCommand, SellerStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public ApproveSellerCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<SellerStatusResponse> Handle(ApproveSellerCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetWithSellerProfileAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.SellerProfile == null)
            throw new InvalidOperationException("User does not have a seller profile.");

        user.SellerProfile.Approve();

        return new SellerStatusResponse(
            user.Id,
            user.SellerProfile.IsApproved,
            "Seller has been approved successfully."
        );
    }
}

/// <summary>
/// Handler for RejectSellerCommand.
/// </summary>
public class RejectSellerCommandHandler : IRequestHandler<RejectSellerCommand, SellerStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public RejectSellerCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<SellerStatusResponse> Handle(RejectSellerCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetWithSellerProfileAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.SellerProfile == null)
            throw new InvalidOperationException("User does not have a seller profile.");

        user.SellerProfile.RevokeApproval();

        return new SellerStatusResponse(
            user.Id,
            user.SellerProfile.IsApproved,
            $"Seller application has been rejected. Reason: {request.Reason}"
        );
    }
}

/// <summary>
/// Handler for SuspendSellerCommand.
/// </summary>
public class SuspendSellerCommandHandler : IRequestHandler<SuspendSellerCommand, SellerStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public SuspendSellerCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<SellerStatusResponse> Handle(SuspendSellerCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetWithSellerProfileAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.SellerProfile == null)
            throw new InvalidOperationException("User does not have a seller profile.");

        user.SellerProfile.RevokeApproval();

        return new SellerStatusResponse(
            user.Id,
            user.SellerProfile.IsApproved,
            $"Seller has been suspended. Reason: {request.Reason}"
        );
    }
}

/// <summary>
/// Handler for ReactivateSellerCommand.
/// </summary>
public class ReactivateSellerCommandHandler : IRequestHandler<ReactivateSellerCommand, SellerStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public ReactivateSellerCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<SellerStatusResponse> Handle(ReactivateSellerCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetWithSellerProfileAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.SellerProfile == null)
            throw new InvalidOperationException("User does not have a seller profile.");

        user.SellerProfile.Approve();

        return new SellerStatusResponse(
            user.Id,
            user.SellerProfile.IsApproved,
            "Seller has been reactivated."
        );
    }
}
