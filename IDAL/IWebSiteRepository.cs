using DTO;
using System;
namespace IDAL
{
    public interface IWebSitesRepository
    {
        Task<IEnumerable<WebSitesDTO>> GetAllWebSitesAsync();
        Task<WebSitesDTO> GetWebSiteByIdAsync(int id);
        Task<WebSitesDTO> AddWebSiteAsync(WebSitesDTO siteDto);
        Task<WebSitesDTO> UpdateWebSiteAsync(int id, WebSitesDTO siteDto);
        Task<bool> DeleteWebSiteAsync(int id);
    }
}

