using Entities.models;

namespace IDAL
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetUserRefreshTokensAsync(int userId);
        Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task<bool> DeleteRefreshTokenAsync(string id);
        Task<bool> DeleteExpiredTokensAsync();
    }
}