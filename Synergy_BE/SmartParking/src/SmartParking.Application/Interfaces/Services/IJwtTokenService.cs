using System.Security.Claims;

namespace SmartParking.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, string role);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
