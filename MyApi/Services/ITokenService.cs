using MyApi.Models;

namespace MyApi.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}
