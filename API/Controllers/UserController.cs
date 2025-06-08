using AutoMapper;
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DAL;
using System.Threading.Tasks;
using Entities.models;
using System.Text.Json;
using System.Text;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("by-id{id}")]
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
    [HttpGet("by-userName{userName}")]
    public async Task<IActionResult> GetUserByUserName(string userName)
    {
        try
        {
            var user = await _usersService.GetUserByUserNameAsync(userName);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while login the user");
        }
    }
    [HttpPost("login")]
    public async Task<IActionResult> GetUserByUserName([FromBody] JsonElement data)
    {
        try
        {
            string email = data.GetProperty("email").GetString();
            string password = data.GetProperty("password").GetString();
            var user1 = await _usersService.Login(email,password);
            if (user1 == null)
                return NotFound();
            return Ok(user1);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while login the user");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody]UsersDTO userDto)
    {
        try
        {
            if (userDto == null)
                return BadRequest("user cannot be null.");
            else
            {
                ObjectId id = ObjectId.GenerateNewId();
                userDto.Id = id.ToString();
                
                //Users convertuser = _mapper.Map<Users>(userDto);
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
        if (!ObjectId.TryParse(id, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");

        var deleted = await _usersService.DeleteUserAsync(objectId);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}