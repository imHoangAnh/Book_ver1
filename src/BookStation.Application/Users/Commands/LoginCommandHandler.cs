using BookStation.Application.Services;
using BookStation.Domain.Repositories;
using MediatR;

namespace BookStation.Application.Users.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check if user is active
        if (user.Status != Domain.Enums.EUserStatus.Active)
        {
            throw new UnauthorizedAccessException("User account is not active.");
        }

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email.Value);

        return new LoginResponse(
            user.Id,
            user.Email.Value,
            token,
            DateTime.UtcNow.AddMinutes(60) // Should match JwtSettings.ExpirationInMinutes
        );
    }
}
