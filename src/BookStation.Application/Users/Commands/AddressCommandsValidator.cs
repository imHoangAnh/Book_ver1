using FluentValidation;

namespace BookStation.Application.Users.Commands;

/// <summary>
/// Validator for CreateAddressCommand.
/// </summary>
public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");

        RuleFor(x => x.Ward)
            .MaximumLength(100).WithMessage("Ward cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Ward));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("District cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.District));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Label)
            .MaximumLength(50).WithMessage("Label cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Label));

        RuleFor(x => x.RecipientName)
            .MaximumLength(100).WithMessage("Recipient name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.RecipientName));

        RuleFor(x => x.RecipientPhone)
            .Matches(@"^[\d\s\-\+\(\)]{7,20}$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrEmpty(x.RecipientPhone));
    }
}

/// <summary>
/// Validator for UpdateAddressCommand.
/// </summary>
public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.AddressId)
            .GreaterThan(0).WithMessage("Address ID must be greater than 0.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");

        RuleFor(x => x.Ward)
            .MaximumLength(100).WithMessage("Ward cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Ward));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("District cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.District));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Label)
            .MaximumLength(50).WithMessage("Label cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Label));

        RuleFor(x => x.RecipientName)
            .MaximumLength(100).WithMessage("Recipient name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.RecipientName));

        RuleFor(x => x.RecipientPhone)
            .Matches(@"^[\d\s\-\+\(\)]{7,20}$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrEmpty(x.RecipientPhone));
    }
}

/// <summary>
/// Validator for DeleteAddressCommand.
/// </summary>
public class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.AddressId)
            .GreaterThan(0).WithMessage("Address ID must be greater than 0.");
    }
}

/// <summary>
/// Validator for SetDefaultAddressCommand.
/// </summary>
public class SetDefaultAddressCommandValidator : AbstractValidator<SetDefaultAddressCommand>
{
    public SetDefaultAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.AddressId)
            .GreaterThan(0).WithMessage("Address ID must be greater than 0.");
    }
}
