using BookStation.Domain.ValueObjects;
using FluentValidation;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Validator for UpdateProfileCommand.
/// </summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleFor(x => x.Phone)
            .Must(BeValidPhoneNumber).WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.AvatarUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Avatar URL is not valid.")
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));

        // Address validation: If Street is provided, City and Country are required
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required when updating address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Street));

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required when updating address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Street));
    }

    private static bool BeValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return true;
            
        return PhoneNumber.TryCreate(phone, out _);
    }
}

/// <summary>
/// Validator for ChangePasswordCommand.
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Custom((password, context) =>
            {
                if (string.IsNullOrEmpty(password)) 
                    return;

                var errors = Password.Validate(password);
                foreach (var error in errors)
                {
                    context.AddFailure("NewPassword", error);
                }
            });
    }
}
