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

        // Create value objects
        var email = Email.Create(request.Email);
        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            phone = PhoneNumber.Create(request.Phone);
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

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

/// <summary>
/// Interface for password hashing service.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
