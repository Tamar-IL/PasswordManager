using AutoMapper;
using DTO;
using IBL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUsersBL _usersService;
    private readonly IMapper _mapper;
    private readonly IMongoDatabase _dbService;

    public UsersController(IUsersBL usersService, IMongoDatabase dbService, IMapper mapper)
    {
        _usersService = usersService;
        _dbService = dbService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _usersService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("by-id/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid ObjectId format.");

            var user = await _usersService.GetUserByIdAsync(objectId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding the user");
        }


    }
    
    [HttpGet("by-userName/{userName}")]
    public async Task<IActionResult> GetUserByUserName(string userName)
    {
        try
        {
            var user = await _usersService.GetUserByEmailAsync(userName);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while login the user");
        }
    }
    ////[HttpPost("login")]
    ////public async Task<IActionResult> GetUserByUserName([FromBody] JsonElement data)
    ////{
    ////    try
    ////    {
    ////        string email = data.GetProperty("email").GetString();
    ////        string password = data.GetProperty("password").GetString();
    ////        var user1 = await _usersService.Login(email,password);
    ////        if (user1 == null)
    ////            return NotFound();
    ////        return Ok(user1);
    ////    }
    ////    catch (Exception ex)
    ////    {
    ////        throw new Exception("An error occurred while login the user");
    ////    }
    ////}
    //[AllowAnonymous]
    //[HttpPost("login")]
    //public async Task<IActionResult> Login([FromBody] JsonElement data)
    //{
    //    try
    //    {
    //        Console.WriteLine($" Session ID: {HttpContext.Session.Id}");
    //        Console.WriteLine($" Session Keys: {string.Join(", ", HttpContext.Session.Keys)}");

    //        string email = data.GetProperty("email").GetString();
    //        string password = data.GetProperty("password").GetString();

    //        var (user, mfaCode) = await _usersService.Login(email, password);

    //        if (user.Id != null)
    //        {
    //            if (!HttpContext.Session.IsAvailable)
    //            {
    //                Console.WriteLine("Session is not available!");
    //                return StatusCode(500, new { message = "Session service unavailable" });
    //            }
    //            await HttpContext.Session.LoadAsync();
    //            string sessionKey = $"mfa_{user.Email}";
    //            HttpContext.Session.SetString(sessionKey, mfaCode);

    //            //HttpContext.Session.SetString($"mfa_{user.Email}", mfaCode);
    //            HttpContext.Session.SetString($"{sessionKey}_expiry",
    //            DateTime.UtcNow.AddMinutes(5).ToString("O"));

    //            await HttpContext.Session.CommitAsync();
    //            var savedValue = HttpContext.Session.GetString(sessionKey);
    //            Console.WriteLine($" Verification - Saved value: {savedValue ?? "NULL"}");

    //            Response.Cookies.Append("userEmail", email, new CookieOptions
    //            {
    //                HttpOnly = false,
    //                Secure = Request.IsHttps,
    //                SameSite = SameSiteMode.Lax,
    //                Expires = DateTimeOffset.UtcNow.AddMinutes(30),
    //                Path = "/"
    //            });
    //            return Ok(new
    //            {
    //                user = user,
    //                mfaRequired = true,
    //                message = "MFA code sent to your email",
    //                sessionId = HttpContext.Session.Id,
    //                debug = new
    //                {
    //                    sessionKey = sessionKey,
    //                    savedCode = savedValue,
    //                    sessionKeys = HttpContext.Session.Keys.ToList()
    //                }
    //            });
    //        }

    //        return BadRequest(new { message = "Invalid credentials" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}
    //[AllowAnonymous]
    //[HttpPost("verify-mfa")]
    //public async Task<IActionResult> VerifyMfa([FromBody] JsonElement data)
    //{
    //    try
    //    {
    //        Console.WriteLine($"Session ID: {HttpContext.Session.Id}");

    //        if (!data.TryGetProperty("email", out var emailElement) ||
    //       !data.TryGetProperty("code", out var codeElement))
    //        {
    //            return BadRequest(new { message = "Email and code are required" });
    //        }
    //        //string email = data.GetProperty("email").GetString();
    //        //string code = data.GetProperty("code").GetString();
    //        string email = emailElement.GetString();
    //        string code = codeElement.GetString();
    //        email = Uri.UnescapeDataString(email);

    //        //var savedCode = HttpContext.Session.GetString($"mfa_{email}");
    //        if (!HttpContext.Session.IsAvailable)
    //        {
    //            Console.WriteLine(" Session is not available in VerifyMfa!");
    //            return BadRequest(new { message = "Session expired. Please login again." });
    //        }
    //        await HttpContext.Session.LoadAsync();
    //        string sessionKey = $"mfa_{email}";
    //        string expiryKey = $"{sessionKey}_expiry";
    //        var savedCode = HttpContext.Session.GetString(sessionKey);
    //        var expiryString = HttpContext.Session.GetString(expiryKey);
    //        if (!string.IsNullOrEmpty(expiryString) &&
    //      DateTime.TryParse(expiryString, out var expiry) &&
    //      expiry < DateTime.UtcNow)
    //        {
    //            HttpContext.Session.Remove(sessionKey);
    //            HttpContext.Session.Remove(expiryKey);
    //            await HttpContext.Session.CommitAsync();
    //            return BadRequest(new { message = "MFA code expired. Please login again." });
    //        }
    //        if (!string.IsNullOrEmpty(savedCode) &&
    //       !string.IsNullOrEmpty(code) &&
    //       savedCode.Trim() == code.Trim())
    //        {
    //            HttpContext.Session.Remove(sessionKey);
    //            HttpContext.Session.Remove(expiryKey);
    //            await HttpContext.Session.CommitAsync();

    //            Response.Cookies.Append("mfaVerified", "true", new CookieOptions
    //            {
    //                HttpOnly = false,
    //                Secure = Request.IsHttps,
    //                SameSite = SameSiteMode.Lax,
    //                Expires = DateTimeOffset.UtcNow.AddHours(24),
    //                Path = "/"
    //            });
    //            Console.WriteLine($" MFA verification successful for {email}");

    //            return Ok(new
    //            {
    //                message = "Login successful",
    //                mfaVerified = true
    //            });

    //        //    if (savedCode != null && _usersService.VerifyMfaCode(savedCode, code))
    //        //{
    //        //    HttpContext.Session.Remove($"mfa_{email}");
    //        //    return Ok(new
    //        //    {
    //        //        message = $"Debug - Input: '{code}' | Saved: '{savedCode}' | Equal: {savedCode == code}",
    //        //        inputCode = code,
    //        //        savedCode = savedCode,
    //        //        isEqual = savedCode == code
    //        //    });
    //            //return Ok(new
    //            //{
    //            //    message = "Login successful",
    //            //    mfaVerified = true
    //            //});
    //        }
    //        return BadRequest(new
    //        {
    //            message = "Invalid MFA code",
    //            debug = new
    //            {
    //                inputCode = code,
    //                inputLength = code?.Length ?? 0,
    //                savedCode = savedCode,
    //                savedLength = savedCode?.Length ?? 0,
    //                isEqual = savedCode?.Trim() == code?.Trim(),
    //                sessionKeys = HttpContext.Session.Keys.ToList()
    //            }
    //        });
    //        //return BadRequest(new { message = "Invalid MFA code" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody]UsersDTO userDto)
    {
        try
        {
            if (userDto == null)
                return BadRequest("user cannot be null.");
            else
            {
                ObjectId 
                    id = ObjectId.GenerateNewId();
                userDto.Id = id.ToString();
                var newUser = await _usersService.AddUserAsync(userDto);
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
            }
        }
        catch(Exception ex)
        {
            throw new Exception("An error occurred while adding the user", ex);

        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, UsersDTO userDto)
    {
        userDto.Id = id;
        if (!ObjectId.TryParse(id, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");

        var updatedUser = await _usersService.UpdateUserAsync(objectId, userDto);
        if (updatedUser == null)
            return NotFound();
        return Ok(updatedUser);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid ObjectId format.");

            var deleted = await _usersService.DeleteUserAsync(objectId);
            if (!deleted)
                throw new Exception();
        }
        catch (Exception ex)
        {
            throw new Exception("worng", ex);
        }
        return NoContent();
    }

}