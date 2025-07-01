using DTO;
using MyProject.Common;
using System.Security.Claims;

namespace IBL
{
    public interface IAuthenticationBL
    {
        Task<(UsersDTO user, string accessToken, string refreshToken)> LoginAsync(string email, string password, string ipAddress = null, string userAgent = null);
        Task<(string accessToken, string refreshToken)> RefreshTokensAsync(string refreshToken, string ipAddress = null, string userAgent = null);
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(UsersDTO user, string ipAddress = null, string userAgent = null);
       

        Task<ClaimsPrincipal> ValidateTokenAsync(string token);

        Task<bool> LogoutAsync(string refreshToken);
        Task<bool> RevokeAllUserTokensAsync(int userId);
        string GenerateCsrfToken();
    }
}