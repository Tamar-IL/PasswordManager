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
using Microsoft.AspNetCore.Authorization;
using IBL.RSAForMasterKey;
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
public class RSAController : ControllerBase
{
    private readonly IRSAencryption _rsaService;
    private readonly IAESEncryptionBL _AESService;
    public RSAController(IRSAencryption rSAencryption)
    {
        _rsaService = rSAencryption;   
    }
   
    [HttpGet("public-key")]
    public IActionResult GetPublicKey()
    {
        try
        {
            // המר מ-XML ל-PEM
            var xmlPublicKey = _rsaService.GetPublicKey();
            Console.WriteLine($"XML key: {xmlPublicKey?.Substring(0, 100)}...");

            // המרה ל-PEM
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(xmlPublicKey);
                byte[] publicKeyBytes = rsa.ExportRSAPublicKey();
                string pemKey = Convert.ToBase64String(publicKeyBytes);

                string formattedPemKey = $"-----BEGIN RSA PUBLIC KEY-----\n{pemKey}\n-----END RSA PUBLIC KEY-----";

                Console.WriteLine($"PEM key: {formattedPemKey.Substring(0, 100)}...");

                return Ok(new { publicKey = formattedPemKey });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
    
    //[HttpGet("AESkey")]
    //public IActionResult GetAESKey()
    //{
    //    try
    //    {
    //       return Ok( _AESService.getKey());
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error: {ex.Message}");
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

}