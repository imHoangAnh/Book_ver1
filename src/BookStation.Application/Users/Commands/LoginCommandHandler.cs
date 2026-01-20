using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStation.Application.Users.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
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

        // Get user with roles
        var userWithRoles = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
        var roles = userWithRoles?.UserRoles.Select(ur => ur.Role!.Name).ToList() ?? new List<string>();

        // Get permissions from all user's roles (RBAC)
        var permissions = new List<string>();
        foreach (var userRole in userWithRoles?.UserRoles ?? [])
        {
            var roleWithPermissions = await _roleRepository.GetWithPermissionsAsync(
                userRole.RoleId, 
                cancellationToken);

            if (roleWithPermissions != null)
            {
                var rolePermissions = roleWithPermissions.RolePermissions
                    .Select(rp => rp.Permission!.Name)
                    .ToList();
                permissions.AddRange(rolePermissions);
            }
        }

        // Remove duplicates
        permissions = permissions.Distinct().ToList();

        // Generate JWT token with roles and permissions
        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email.Value, roles, permissions);

        return new LoginResponse(
            user.Id,
            user.Email.Value,
            token,
            DateTime.UtcNow.AddMinutes(60), // Should match JwtSettings.ExpirationInMinutes
            roles
        );
    }
}
