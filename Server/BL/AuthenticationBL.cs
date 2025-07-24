using AutoMapper;
using BL.RSAForMasterKay;
using DTO;
using Entities.models;
using IBL.RSAForMasterKey;
using IDAL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyProject.Common;
using MongoDB.Bson;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using IBL;

namespace BL.NewFolder
{
    public class AuthenticationBL : IAuthenticationBL
    {
        private readonly ILogger<AuthenticationBL> _logger;
        private readonly IUsersRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly IRSAencryption _RSAencryption;
        private readonly MySetting _mySetting;

        public AuthenticationBL(IRSAencryption RSAencryption, IUsersRepository userRepository, IRefreshTokenRepository refreshTokenRepository, ILogger<AuthenticationBL> logger, IMapper mapper, IOptions<MySetting> mySetting)
        {
            _logger = logger;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _RSAencryption = RSAencryption;
            _mySetting = mySetting.Value;
        }

        public async Task<(UsersDTO user, string accessToken, string refreshToken)> LoginAsync(string email, string password, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var allUsers = await _userRepository.GetAllUsersAsync();

                foreach (Users user in allUsers)
                {
                    if (user.Email == email)
                    {
                        string decPas = _RSAencryption.Decrypt(user.Password, _RSAencryption.GetPrivateKayFromSecureStorge());

                        if (decPas == password)
                        {
                            var userDto = _mapper.Map<UsersDTO>(user);
                            userDto.Password = null;

                            var (accessToken, refreshToken) = await GenerateTokensAsync(userDto, ipAddress, userAgent);

                            _logger.LogInformation($"User {email} logged in successfully");
                            return (userDto, accessToken, refreshToken);
                        }
                        else
                        {
                            throw new Exception("Invalid email or password");
                        }
                    }
                }

                _logger.LogWarning($"Failed login attempt for email {email}");
                throw new Exception("Invalid email or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for email {email}");
                throw;
            }
        }

        public async Task<(string accessToken, string refreshToken)> RefreshTokensAsync(string refreshToken, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var storedToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshToken);

                if (storedToken == null || !storedToken.IsValid)
                {
                    _logger.LogWarning("Invalid refresh token attempt");
                    throw new Exception("Invalid refresh token");
                }

                var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(storedToken.UserId.ToString()));
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var userDto = _mapper.Map<UsersDTO>(user);
                userDto.Password = null;

                storedToken.Revoke("Token refreshed");
                await _refreshTokenRepository.UpdateRefreshTokenAsync(storedToken);

                var newTokens = await GenerateTokensAsync(userDto, ipAddress, userAgent);

                _logger.LogInformation($"Tokens refreshed for user {user.Email}");
                return newTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing tokens");
                throw;
            }
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_mySetting.JwtSecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _mySetting.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _mySetting.JwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                var storedToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshToken);

                if (storedToken == null)
                {
                    return false;
                }

                storedToken.Revoke("Manual logout");
                await _refreshTokenRepository.UpdateRefreshTokenAsync(storedToken);

                _logger.LogInformation($"User {storedToken.UserId} logged out");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout");
                throw;
            }
        }

        public async Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            try
            {
                var userTokens = await _refreshTokenRepository.GetUserRefreshTokensAsync(userId);

                foreach (var token in userTokens)
                {
                    if (token.IsActive)
                    {
                        token.Revoke("Revoke all tokens");
                        await _refreshTokenRepository.UpdateRefreshTokenAsync(token);
                    }
                }

                _logger.LogInformation($"All tokens revoked for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while revoking all tokens for user {userId}");
                throw;
            }
        }

        public string GenerateCsrfToken()
        {
            try
            {
                var randomBytes = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSRF token");
                throw;
            }
        }

        public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(UsersDTO user, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id.ToString(),
                    Token = refreshToken,
                    //ExpiresAt = DateTime.UtcNow.AddDays(_mySetting.RefreshTokenDays),
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _refreshTokenRepository.AddRefreshTokenAsync(refreshTokenEntity);

                return (accessToken, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while generating tokens for user {user.Email}");
                throw;
            }
        }

        private string GenerateAccessToken(UsersDTO user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_mySetting.JwtSecretKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_mySetting.AccessTokenMinutes),
                Issuer = _mySetting.JwtIssuer,
                Audience = _mySetting.JwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}