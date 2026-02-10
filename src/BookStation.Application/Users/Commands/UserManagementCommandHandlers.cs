using BookStation.Domain.Enums;
using BookStation.Domain.Repositories;
using MediatR;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Handler for VerifyUserCommand.
/// </summary>
public class VerifyUserCommandHandler : IRequestHandler<VerifyUserCommand, UserStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public VerifyUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserStatusResponse> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Verify();

        return new UserStatusResponse(
            user.Id,
            user.Status.ToString(),
            "User has been verified successfully."
        );
    }
}

/// <summary>
/// Handler for BanUserCommand.
/// </summary>
public class BanUserCommandHandler : IRequestHandler<BanUserCommand, UserStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public BanUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserStatusResponse> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Ban(request.Reason);

        return new UserStatusResponse(
            user.Id,
            user.Status.ToString(),
            $"User has been banned. Reason: {request.Reason}"
        );
    }
}

/// <summary>
/// Handler for UnbanUserCommand.
/// </summary>
public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, UserStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public UnbanUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserStatusResponse> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.Status != EUserStatus.Banned)
            throw new InvalidOperationException("User is not banned.");

        user.Activate();

        return new UserStatusResponse(
            user.Id,
            user.Status.ToString(),
            "User has been unbanned."
        );
    }
}

/// <summary>
/// Handler for SuspendUserCommand.
/// </summary>
public class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand, UserStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public SuspendUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserStatusResponse> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Suspend(request.Reason, request.SuspendUntil);

        return new UserStatusResponse(
            user.Id,
            EUserStatus.Suspended.ToString(),
            $"User has been suspended until {request.SuspendUntil}. Reason: {request.Reason}"
        );
    }
}

/// <summary>
/// Handler for ActivateUserCommand.
/// </summary>
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public ActivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserStatusResponse> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Activate();

        return new UserStatusResponse(
            user.Id,
            user.Status.ToString(),
            "User has been activated."
        );
    }
}

/// <summary>
/// Handler for DeactivateUserCommand.
/// </summary>
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, UserStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public DeactivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserStatusResponse> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Deactivate();

        return new UserStatusResponse(
            user.Id,
            user.Status.ToString(),
            "User has been deactivated."
        );
    }
}
