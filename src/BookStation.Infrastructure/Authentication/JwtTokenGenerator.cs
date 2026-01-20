using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookStation.Infrastructure.Authentication;

/// <summary>
/// JWT token generator service.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(long userId, string email, IEnumerable<string> roles, IEnumerable<string>? permissions = null);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(
        long userId,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
        };

        // Add roles
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add permissions (for permission-based authorization)
        if (permissions != null)
        {
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
