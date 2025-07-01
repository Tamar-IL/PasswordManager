using DTO;
using Microsoft.AspNetCore.Mvc;
using IBL;
using MongoDB.Driver;
using AutoMapper;
using MongoDB.Bson;
using Entities.models;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WebSitesController : ControllerBase
{
    private readonly IWebSitesBL _webSitesService;
    private readonly IMapper _mapper;
    private readonly IMongoDatabase _dbService;
    public WebSitesController(IWebSitesBL webSitesService, IMapper mapper, IMongoDatabase dbService)
    {
        _webSitesService = webSitesService;
        _dbService = dbService;
        _mapper = mapper;

    }

    [HttpGet]
    public async Task<IActionResult> GetAllSites()
    {
        var sites = await _webSitesService.GetAllWebSitesAsync();
        return Ok(sites);

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSiteById(string id)
    {
        if (!ObjectId.TryParse(id, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");
        var site = await _webSitesService.GetWebSiteByIdAsync(objectId);
        if (site == null)
            return NotFound();
        return Ok(site);
    }
    [HttpGet("bysiteName{sitename}")]
    public async Task<IActionResult> GetWebSitesBySiteNAme(string siteName)
    {
        if (siteName == null)
            return BadRequest("cant be null.");
        WebSitesDTO site = await _webSitesService.GetWebSitesByNameAsync(siteName);
        if (site == null)
            return NotFound();
        return Ok(site);
    }
    [HttpGet("by-url{url}")]
    public async Task<IActionResult> GetPasswordByUrlSite(string url)
    {

        var password = await _webSitesService.GetPasswordByUrlSiteAsync(url);
        if (password == null)
            return NotFound();
        return Ok(password);
    }
    [HttpPost]
    public async Task<IActionResult> AddSite(WebSitesDTO siteDto)
    {
        try
        {
            if (siteDto == null)
                return BadRequest("site cannot be null.");
            else
            {
                ObjectId id = ObjectId.GenerateNewId();
                siteDto.Id = id.ToString();
                var newUser = await _webSitesService.AddWebSiteAsync(siteDto);
                return Ok(newUser);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding the site", ex);

        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSite(string id, WebSitesDTO siteDto)
    {
        siteDto.Id = id;
        if (!ObjectId.TryParse(id, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");
        var updatedSite = await _webSitesService.UpdateWebSiteAsync(objectId, siteDto);
        if (updatedSite == null)
            return NotFound();
        return Ok(updatedSite);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSite(string id)
    {
        if (!ObjectId.TryParse(id, out ObjectId objectId))
            return BadRequest("Invalid ObjectId format.");
        var deleted = await _webSitesService.DeleteWebSiteAsync(objectId);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

}
