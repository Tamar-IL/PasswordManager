using AutoMapper;
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUsers _usersService;
    private readonly IMapper _mapper;
    private readonly IMongoDatabase _dbService;
    public UsersController(IUsers usersService,IMongoDatabase dbService,IMapper mapper)
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
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _usersService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(UsersDTO userDto)
    {
        var newUser = await _usersService.AddUserAsync(userDto);
        return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, newUser);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UsersDTO userDto)
    {
        var updatedUser = await _usersService.UpdateUserAsync(id, userDto);
        if (updatedUser == null)
            return NotFound();
        return Ok(updatedUser);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await _usersService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
    
}