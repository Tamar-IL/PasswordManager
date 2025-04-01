using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IBL
{
    public interface IWebSites
    {
        Task<IEnumerable<WebSitesDTO>> GetAllWebSitesAsync();
        Task<WebSitesDTO> GetWebSiteByIdAsync(int id);
        Task<WebSitesDTO> AddWebSiteAsync(WebSitesDTO siteDto);
        Task<WebSitesDTO> UpdateWebSiteAsync(int id, WebSitesDTO siteDto);
        Task<bool> DeleteWebSiteAsync(int id);

    }
}
