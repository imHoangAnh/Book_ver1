namespace BookStation.Application.Services;

/// <summary>
/// JWT token generator service interface.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email);
}
