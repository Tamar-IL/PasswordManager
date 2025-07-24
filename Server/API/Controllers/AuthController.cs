using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyProject.Common;
using IBL;
using DTO;
using System.Security.Claims;
using IBL.RSAForMasterKey;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationBL _authenticationBL;
        private readonly MySetting _mySetting;
        private readonly ILogger<AuthController> _logger;
        private readonly IUsersBL _usersService;

        public AuthController(IAuthenticationBL authenticationBL,IOptions<MySetting> mySetting,ILogger<AuthController> logger, IUsersBL usersService)
        {
            _authenticationBL = authenticationBL;
            _mySetting = mySetting.Value;
            _logger = logger;
            _usersService = usersService;
        }

        //[HttpPost("login")]
        //public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        //        {
        //            return BadRequest("אימייל וסיסמה נדרשים");
        //        }

        //        var ipAddress = GetClientIpAddress();
        //        var userAgent = Request.Headers["User-Agent"].ToString();

        //        var (user, accessToken, refreshToken) = await _authenticationBL.LoginAsync(
        //            request.Email,
        //            request.Password,
        //            ipAddress,
        //            userAgent);

        //        SetRefreshTokenCookie(refreshToken);

        //        var response = new LoginResponse
        //        {
        //            User = user,
        //            AccessToken = accessToken,
        //            TokenExpiry = DateTime.UtcNow.AddMinutes(_mySetting.AccessTokenMinutes),
        //            Message = "התחברות בוצעה בהצלחה"
        //        };

        //        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Login failed for email {Email}", request.Email);
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //}
        //[AllowAnonymous]
        //[HttpPost("login")]
        //public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)

        //{ _logger.LogInformation(" LOGIN STARTED - Request received for {Email}", request.Email);

        //    try
        //    {
        //        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        //        {
        //            return BadRequest("אימייל וסיסמה נדרשים");
        //        }

        //        var ipAddress = GetClientIpAddress();
        //        var userAgent = Request.Headers["User-Agent"].ToString();

        //        // קבל את המשתמש והטוקנים
        //        var (user, accessToken, refreshToken) = await _authenticationBL.LoginAsync(
        //            request.Email,
        //            request.Password,
        //            ipAddress,
        //            userAgent);

        //        // וודא שיש refresh token
        //        if (string.IsNullOrEmpty(refreshToken))
        //        {
        //            _logger.LogError("No refresh token generated for user {Email}", request.Email);
        //            return StatusCode(500, "שגיאה ביצירת refresh token");
        //        }


        //        SetRefreshTokenCookie(refreshToken);


        //        _logger.LogInformation("Refresh token cookie set for user {Email}", request.Email);

        //        var response = new LoginResponse
        //        {
        //            User = user,
        //            AccessToken = accessToken,
        //            TokenExpiry = DateTime.UtcNow.AddMinutes(_mySetting.AccessTokenMinutes),
        //            Message = "התחברות בוצעה בהצלחה"
        //        };

        //        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Login failed for email {Email}", request.Email);
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    _logger.LogInformation("Refresh token cookie set for user {Email}", request.Email);

        //}
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {            
            _logger.LogInformation("LOGIN STARTED - Request received for {Email}", request?.Email);

            try
            {                
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("אימייל וסיסמה נדרשים");
                }                
                var (user, mfaCode) = await _usersService.Login(request.Email, request.Password);

                if (user.Id != null)
                {
                    
                    return Ok(new
                    {
                        user = user,
                        mfaRequired = true,
                        message = "MFA code sent to your email",
                        tempToken = "temp_" + user.Id // token זמני לזיהוי
                    });
                }

                return BadRequest(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email {Email}", request.Email);
                return Unauthorized(new { message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpPost("verify-mfa")]
        public async Task<ActionResult<LoginResponse>> VerifyMfa([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Email and code are required" });
                }

                var isValidMfa = _usersService.VerifyTimeBasedMfaCode(request.Email, request.Password);

                if (!isValidMfa)
                {
                    return BadRequest(new { message = "Invalid MFA code" });
                }

                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                
                var userDto = await _usersService.GetUserByEmailAsync(request.Email);
                var (accessToken, refreshToken) = await _authenticationBL.GenerateTokensAsync(userDto, ipAddress, userAgent);

                SetRefreshTokenCookie(refreshToken);

                return Ok(new LoginResponse
                {
                    User = userDto,
                    AccessToken = accessToken,
                    TokenExpiry = DateTime.UtcNow.AddMinutes(_mySetting.AccessTokenMinutes),
                    Message = "Login successful with MFA"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MFA verification failed");
                return BadRequest(new { message = "Invalid MFA code" });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshResponse>> RefreshTokens()
        {
            try
            {

                var refreshToken = GetRefreshTokenFromCookie();

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { message = "Refresh token לא נמצא" });
                }

                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var (newAccessToken, newRefreshToken) = await _authenticationBL.RefreshTokensAsync(
                    refreshToken,
                    ipAddress,
                    userAgent);

                SetRefreshTokenCookie(newRefreshToken);

                var response = new RefreshResponse
                {
                    AccessToken = newAccessToken,
                    TokenExpiry = DateTime.UtcNow.AddMinutes(_mySetting.AccessTokenMinutes)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token refresh failed");
                ClearRefreshTokenCookie();
                return Unauthorized(new { message = "חידוש טוקן נכשל" });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var refreshToken = GetRefreshTokenFromCookie();

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _authenticationBL.LogoutAsync(refreshToken);
                }

                ClearRefreshTokenCookie();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("User {UserId} logged out", userId);

                return Ok(new { message = "התנתקות בוצעה בהצלחה" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return BadRequest(new { message = "שגיאה בהתנתקות" });
            }
        }

        [HttpPost("revoke-all")]
        public async Task<ActionResult> RevokeAllTokens()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("מזהה משתמש לא תקין");
                }

                await _authenticationBL.RevokeAllUserTokensAsync(userId);
                ClearRefreshTokenCookie();

                _logger.LogInformation("All tokens revoked for user {UserId}", userId);
                return Ok(new { message = "כל הטוקנים בוטלו בהצלחה" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke all tokens");
                return BadRequest(new { message = "שגיאה בביטול הטוקנים" });
            }
        }

        [HttpGet("csrf")]
        public ActionResult<CsrfResponse> GetCsrfToken()
        {
            try
            {
                var csrfToken = _authenticationBL.GenerateCsrfToken();
                SetCsrfTokenCookie(csrfToken);

                return Ok(new CsrfResponse { CsrfToken = csrfToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate CSRF token");
                return BadRequest(new { message = "שגיאה ביצירת CSRF token" });
            }
        }

        [HttpGet("validate")]
        public ActionResult<ValidateResponse> ValidateToken()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

                return Ok(new ValidateResponse
                {
                    IsValid = true,
                    UserId = userIdClaim,
                    Email = emailClaim,
                    Message = "טוקן תקף"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
                return Unauthorized(new { message = "טוקן לא תקף" });
            }
        }

        //private void SetRefreshTokenCookie(string refreshToken)
        //{
        //    var cookieOptions = new CookieOptions
        //    {
        //        HttpOnly = _mySetting.HttpOnlyCookies,
        //        Secure = _mySetting.SecureCookiesOnly && _mySetting.RequireHttps,
        //        SameSite = ParseSameSite(_mySetting.CookieSameSite),
        //        Expires = DateTime.UtcNow.AddDays(_mySetting.RefreshTokenDays),
        //        IsEssential = true,
        //        Path = "/"
        //    };

        //    Response.Cookies.Append(_mySetting.RefreshTokenCookieName, refreshToken, cookieOptions);
        //}
        private void SetRefreshTokenCookie(string refreshToken)
        {
            _logger.LogInformation(" MySetting.RefreshTokenCookieName: '{CookieName}'", _mySetting.RefreshTokenCookieName);
            //_logger.LogInformation(" MySetting.RefreshTokenDays: {Days}", _mySetting.RefreshTokenDays);

            _logger.LogInformation(" Setting refresh token cookie: {TokenStart}...",
                refreshToken.Substring(0, Math.Min(10, refreshToken.Length)));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Lax, 
                //Expires = DateTime.UtcNow.AddDays(_mySetting.RefreshTokenDays),
                IsEssential = true,
                Path = "/"
            };

            var cookieName = _mySetting.RefreshTokenCookieName ?? "refreshToken";



            _logger.LogInformation(" Cookie name: {CookieName}", cookieName);
            _logger.LogInformation(" Cookie settings: HttpOnly={HttpOnly}, Secure={Secure}, SameSite={SameSite}, Expires={Expires}",
                cookieOptions.HttpOnly, cookieOptions.Secure, cookieOptions.SameSite, cookieOptions.Expires);

            Response.Cookies.Append(cookieName, refreshToken, cookieOptions);

            _logger.LogInformation(" Refresh token cookie set successfully");
        }
        private void SetCsrfTokenCookie(string csrfToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = false, // CSRF token צריך להיות נגיש ל-JavaScript
                Secure = _mySetting.SecureCookiesOnly && _mySetting.RequireHttps,
                SameSite = ParseSameSite(_mySetting.CookieSameSite),
                Expires = DateTime.UtcNow.AddHours(_mySetting.CsrfTokenValidHours),
                IsEssential = true,
                Path = "/"
            };

            Response.Cookies.Append(_mySetting.CsrfTokenCookieName, csrfToken, cookieOptions);
        }

        private string GetRefreshTokenFromCookie()
        {

            var cookieName = _mySetting.RefreshTokenCookieName ?? "refreshToken";
            var cookie = Request.Cookies[cookieName];

            _logger.LogInformation(" Looking for cookie: '{CookieName}', Found: {Found}", cookieName, cookie != null);
            _logger.LogInformation(" All request cookies: {Cookies}", string.Join(", ", Request.Cookies.Keys));

            return cookie;
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete(_mySetting.RefreshTokenCookieName, new CookieOptions
            {
                Path = "/",
                Secure = _mySetting.SecureCookiesOnly && _mySetting.RequireHttps,
                SameSite = ParseSameSite(_mySetting.CookieSameSite)
            });
        }

        private string GetClientIpAddress()
        {
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private SameSiteMode ParseSameSite(string sameSite)
        {
            return sameSite?.ToLower() switch
            {
                "strict" => SameSiteMode.Strict,
                "lax" => SameSiteMode.Lax,
                "none" => SameSiteMode.None,
                _ => SameSiteMode.Strict
            };
        }
       
    }
}


    // DTOs for requests and responses
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public UsersDTO User { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RefreshResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
    }

    public class CsrfResponse
    {
        public string CsrfToken { get; set; } = string.Empty;
    }

    public class ValidateResponse
    {
        public bool IsValid { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
    
