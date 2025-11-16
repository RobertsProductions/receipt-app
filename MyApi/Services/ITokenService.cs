using MyApi.Models;

namespace MyApi.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
