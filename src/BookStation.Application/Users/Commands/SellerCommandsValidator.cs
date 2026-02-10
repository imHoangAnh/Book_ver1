using FluentValidation;

namespace BookStation.Application.Users.Commands;

public class ApproveSellerCommandValidator : AbstractValidator<ApproveSellerCommand>
{
    public ApproveSellerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
    }
}

public class RejectSellerCommandValidator : AbstractValidator<RejectSellerCommand>
{
    public RejectSellerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.");
    }
}

public class SuspendSellerCommandValidator : AbstractValidator<SuspendSellerCommand>
{
    public SuspendSellerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.");
    }
}

public class ReactivateSellerCommandValidator : AbstractValidator<ReactivateSellerCommand>
{
    public ReactivateSellerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Invalid User Id.");
    }
}
