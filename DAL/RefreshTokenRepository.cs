using Entities.models;
using IDAL;
using MongoDB.Driver;

namespace DAL
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly MongoDbService _dbService;

        public RefreshTokenRepository(MongoDbService dbService)
        {
            _dbService = dbService;
        }

        public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken.Id))
            {
                refreshToken.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            }

            await _dbService.GetCollection<RefreshToken>("RefreshTokens").InsertOneAsync(refreshToken);
            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _dbService.GetCollection<RefreshToken>("RefreshTokens")
                .Find(rt => rt.Token == token).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetUserRefreshTokensAsync(int userId)
        {
            var userIdString = userId.ToString();
            return await _dbService.GetCollection<RefreshToken>("RefreshTokens")
                .Find(rt => rt.UserId == userIdString).ToListAsync();
        }

        public async Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            var result = await _dbService.GetCollection<RefreshToken>("RefreshTokens")
                .ReplaceOneAsync(rt => rt.Id == refreshToken.Id, refreshToken);

            return result.IsAcknowledged ? refreshToken : null;
        }

        public async Task<bool> DeleteRefreshTokenAsync(string id)
        {
            var result = await _dbService.GetCollection<RefreshToken>("RefreshTokens")
                .DeleteOneAsync(rt => rt.Id == id);

            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteExpiredTokensAsync()
        {
            var result = await _dbService.GetCollection<RefreshToken>("RefreshTokens")
                .DeleteManyAsync(rt => rt.ExpiresAt < DateTime.UtcNow || !rt.IsActive);

            return result.DeletedCount > 0;
        }
    }
}