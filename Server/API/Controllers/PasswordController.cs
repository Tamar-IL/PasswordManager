using Microsoft.AspNetCore.Mvc;
using IBL;
using DTO;
using AutoMapper;
using BL;
using MongoDB.Driver;
using DAL;
using MongoDB.Bson;
using SharpCompress.Common;
using Entities.models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class PasswordsController : ControllerBase
{
    private readonly IPasswordsBL _passwordsService;
    private readonly IMapper _mapper;
    private readonly MongoDbService _dbService;
   
    public PasswordsController(IPasswordsBL passwordsService, MongoDbService dbService, IMapper mapper)
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
    [HttpGet("generate-xml-keys")]
    public IActionResult GenerateXmlKeys()
    {
        using (RSA rsa = RSA.Create(2048))
        {
            string publicKeyXml = rsa.ToXmlString(false);  // מפתח ציבורי
            string privateKeyXml = rsa.ToXmlString(true);   // מפתח פרטי

            return Ok(new
            {
                PublicKey = publicKeyXml,
                PrivateKey = privateKeyXml
            });
        }
    }
   
    [HttpGet("{pasId}")]
    public async Task<IActionResult> GetPasswordById(string pasId, [FromQuery] string publicKey) 
    {
        if (!ObjectId.TryParse(pasId, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");
        var password = await _passwordsService.GetPasswordByIdAsync(objectId,publicKey);
        if (password == null)
            return NotFound();
        return Ok(new { Password = password });
    }
    
    [HttpGet("bysite{siteid}")]
    public async Task<IActionResult> GetPasswordBySiteId(string siteid)
    {
        if (!ObjectId.TryParse(siteid, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");
        var password = await _passwordsService.GetPasswordBySiteIdAsync(objectId);
        if (password == null)
            return NotFound();
        return Ok(password);
    }
   
    [HttpGet("byemail/{id}")]
    public async Task<IActionResult> GetAllPasswordsForUserByUserIdAsync(string id)
    {
        //string decodedPublicKey = Uri.UnescapeDataString(publicKey);

        var password = await _passwordsService.GetAllPasswordsForUserByUserIdAsync(id);
        if (password == null)
            return NotFound();
        return Ok(password);
    }

    [HttpPost]
    public async Task<IActionResult> AddPassword([FromBody]PasswordsDTO NewPassword, [FromQuery] string url)
    {
        try
        {
            if (NewPassword == null)
                return BadRequest("Password  cannot be null.");
            else
            {
                var password = await _passwordsService.AddPasswordAsync(NewPassword,url);

                //await _dbService.GetCollection<Passwords>("passwords");

                return Ok(password);
            }
            //}
        }
        catch (InvalidOperationException ex) // שגיאת כפילות
        {
            return Conflict(new
            {
                message = ex.Message,
                errorCode = "DUPLICATE_PASSWORD"
            }); // HTTP 409 - Conflict
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding the password", ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePassword(string id, PasswordsDTO passwordDto)
    {
        passwordDto.Id = id;
        if (!ObjectId.TryParse(id, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");
        var updatedPassword = await _passwordsService.UpdatePasswordAsync(objectId, passwordDto);
        if (updatedPassword == null)
            return NotFound();
        return Ok(updatedPassword);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePassword(string id)
    {
        if (!ObjectId.TryParse(id, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");
        var deleted = await _passwordsService.DeletePasswordAsync(objectId);
        if (!deleted)
            return NotFound();
        return Ok();
    }
}
