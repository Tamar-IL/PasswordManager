using AutoMapper;
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using DAL;
using System.Threading.Tasks;
using Entities.models;

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

    [HttpGet("{id}")]
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

    [HttpPost]
    public async Task<IActionResult> AddUser(UsersDTO userDto)
    {
        try
        {
            if (userDto == null)
                return BadRequest("user cannot be null.");
            else
            {
                ObjectId id = ObjectId.GenerateNewId();
                userDto.Id = id.ToString();
                Users convertuser = _mapper.Map<Users>(userDto);
                var newUser = await _usersService.AddUserAsync(userDto);
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, convertuser);
            }
        }
        catch(Exception ex)
        {
            throw new Exception("An error occurred while adding the password", ex);

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