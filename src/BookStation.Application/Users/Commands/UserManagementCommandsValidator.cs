using FluentValidation;

namespace BookStation.Application.Users.Commands;

public class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
{
    public VerifyUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
    }
}

public class BanUserCommandValidator : AbstractValidator<BanUserCommand>
{
    public BanUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.");
    }
}

public class UnbanUserCommandValidator : AbstractValidator<UnbanUserCommand>
{
    public UnbanUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
    }
}

public class SuspendUserCommandValidator : AbstractValidator<SuspendUserCommand>
{
    public SuspendUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.");
        RuleFor(x => x.SuspendUntil)
            .Must((command, suspendUntil) => !suspendUntil.HasValue || suspendUntil.Value > DateTime.UtcNow)
            .WithMessage("Suspension end date must be in the future.");
    }
}

public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
    }
}

public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Invalid User Id.");
    }
}
