using Identity.API.Models;
using System.Security.Claims;

namespace Identity.API.Interfaces
{
    /// <summary>
    /// Интерфейс за JWT операции
    /// </summary>
    public interface IJwtService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user, List<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool ValidateRefreshToken(ApplicationUser user, string refreshToken);
    }
}
