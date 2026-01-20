using FluentValidation;

namespace BookStation.Application.Users.Commands;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\s\-\.]{9,20}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
