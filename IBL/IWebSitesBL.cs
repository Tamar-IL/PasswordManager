using DTO;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IBL
{
    public interface IWebSitesBL
    {
        Task<IEnumerable<WebSitesDTO>> GetAllWebSitesAsync();
        Task<WebSitesDTO> GetWebSiteByIdAsync(ObjectId id);
        Task<WebSitesDTO> GetWebSitesByNameAsync(string name);
        Task<WebSitesDTO> AddWebSiteAsync(WebSitesDTO siteDto);
        Task<WebSitesDTO> UpdateWebSiteAsync(ObjectId id, WebSitesDTO siteDto);
        Task<bool> DeleteWebSiteAsync(ObjectId id);
    }
}
