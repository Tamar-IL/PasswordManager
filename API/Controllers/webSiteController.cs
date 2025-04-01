using DTO;
using Microsoft.AspNetCore.Mvc;
using IBL;
using MongoDB.Driver;
using AutoMapper;

[ApiController]
[Route("api/[controller]")]
public class WebSitesController : ControllerBase
{
    private readonly IWebSites _webSitesService;
    private readonly IMapper _mapper;
    private readonly IMongoDatabase _dbService;
    public WebSitesController(IWebSites webSitesService, IMapper mapper, IMongoDatabase dbService)
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
    public async Task<IActionResult> GetSiteById(int id)
    {
        var site = await _webSitesService.GetWebSiteByIdAsync(id);
        if (site == null)
            return NotFound();
        return Ok(site);
    }

    [HttpPost]
    public async Task<IActionResult> AddSite(WebSitesDTO siteDto)
    {
        var newSite = await _webSitesService.AddWebSiteAsync(siteDto);
        return CreatedAtAction(nameof(GetSiteById), new { id = newSite.SiteId }, newSite);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSite(int id, WebSitesDTO siteDto)
    {
        var updatedSite = await _webSitesService.UpdateWebSiteAsync(id, siteDto);
        if (updatedSite == null)
            return NotFound();
        return Ok(updatedSite);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSite(int id)
    {
        var deleted = await _webSitesService.DeleteWebSiteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
