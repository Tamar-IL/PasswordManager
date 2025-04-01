using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using IBL;
using IDAL;

namespace BL
{
    public class WebSites : IWebSites
    {
        private readonly IWebSitesRepository _webSitesRepository;

        // מקבל את הריפוזיטורי דרך הזרקת תלויות
        public WebSites(IWebSitesRepository webSitesRepository)
        {
            _webSitesRepository = webSitesRepository;
        }

        public async Task<IEnumerable<WebSitesDTO>> GetAllWebSitesAsync()
        {
            return await _webSitesRepository.GetAllWebSitesAsync();
        }

        public async Task<WebSitesDTO> GetWebSiteByIdAsync(int id)
        {
            return await _webSitesRepository.GetWebSiteByIdAsync(id);
        }

        public async Task<WebSitesDTO> AddWebSiteAsync(WebSitesDTO siteDto)
        {
            return await _webSitesRepository.AddWebSiteAsync(siteDto);
        }

        public async Task<WebSitesDTO> UpdateWebSiteAsync(int id, WebSitesDTO siteDto)
        {
            return await _webSitesRepository.UpdateWebSiteAsync(id, siteDto);
        }

        public async Task<bool> DeleteWebSiteAsync(int id)
        {
            return await _webSitesRepository.DeleteWebSiteAsync(id);
        }
    }
}
