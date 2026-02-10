using BookStation.Application.Services;
using BookStation.Domain.Entities.UserAggregate;
using BookStation.Domain.Repositories;
using BookStation.Domain.ValueObjects;
using MediatR;

namespace BookStation.Application.Users.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Check if email is already in use
        var isEmailUnique = await _userRepository.IsEmailUniqueAsync(request.Email, cancellationToken);
        if (!isEmailUnique)
        {
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
        }

        // Create value objects - Domain layer validates the values
        var email = Email.Create(request.Email);
        var phone = string.IsNullOrWhiteSpace(request.Phone) 
            ? null 
            : PhoneNumber.Create(request.Phone);

        // Create Password value object - throws ValidationException if invalid
        // This ensures password validation rules are enforced at Domain level
        var password = Password.Create(request.Password);

        // Hash password using the service - returns PasswordHash value object
        var passwordHash = _passwordHasher.HashPassword(password);

        // Create user entity
        var user = User.Create(email, passwordHash, request.FullName, phone);

        // Assign default "User" role
        // TODO: Get role from repository and assign

        // Save user
        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterUserResponse(    
            user.Id,
            user.Email.Value,
            user.IsVerified
        );
    }
}

