using BookStation.Domain.ValueObjects;
using FluentValidation;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Validator for RegisterUserCommand.
/// Performs input validation at Application layer.
/// Business rules are enforced by Value Objects in Domain layer.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // 1. Email: Delegate to Email Value Object
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
            .Must(BeValidEmail).WithMessage("Invalid email format.");

        // 2. Password: Delegate to Password Value Object
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Custom((password, context) =>
            {
                if (string.IsNullOrEmpty(password)) 
                    return;

                var errors = Password.Validate(password);
                foreach (var error in errors)
                {
                    context.AddFailure("Password", error);
                }
            });

        // 3. ConfirmPassword
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        // 4. FullName
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters.");

        // 5. Phone: Delegate to PhoneNumber Value Object
        RuleFor(x => x.Phone)
            .Must(BeValidPhoneNumber).WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }

    private static bool BeValidEmail(string email)
    {
        return Email.TryCreate(email, out _);
    }

    private static bool BeValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return true;
            
        return PhoneNumber.TryCreate(phone, out _);
    }
}
