using Microsoft.AspNetCore.Mvc;
using IBL;
using DTO;
using AutoMapper;
using BL;
using MongoDB.Driver;
using DAL;

[ApiController]
[Route("api/[controller]")]
public class PasswordsController : ControllerBase
{
    private readonly IPasswords _passwordsService;
    private readonly IMapper _mapper;
    private readonly MongoDbService _dbService;

    public PasswordsController(IPasswords passwordsService, MongoDbService dbService, IMapper mapper)
    {
        _passwordsService = passwordsService;
        _mapper = mapper;
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPasswords()
    {
        var passwords = await _passwordsService.GetAllPasswordsAsync();
        return Ok(passwords);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPasswordById(int id)
    {
        var password = await _passwordsService.GetPasswordByIdAsync(id);
        if (password == null)
            return NotFound();
        return Ok(password);
    }

    [HttpPost]
    public async Task<IActionResult> AddPassword(PasswordsDTO passwordDto)
    {
        Console.WriteLine($"Received DTO: {System.Text.Json.JsonSerializer.Serialize(passwordDto)}");

        var password = _mapper.Map<Passwords>(passwordDto);

        Console.WriteLine($"Mapped Entity: {System.Text.Json.JsonSerializer.Serialize(password)}");

        await _dbService.GetCollection<Passwords>("passwords").InsertOneAsync(password);

        return Ok(passwordDto);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePassword(int id, PasswordsDTO passwordDto)
    {
        var updatedPassword = await _passwordsService.UpdatePasswordAsync(id, passwordDto);
        if (updatedPassword == null)
            return NotFound();
        return Ok(updatedPassword);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePassword(int id)
    {
        var deleted = await _passwordsService.DeletePasswordAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
