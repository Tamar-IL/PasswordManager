using DTO;
using Entities.models;
using MongoDB.Bson;
using System;
namespace IDAL
{
    public interface IWebSitesRepository
    {
        Task<IEnumerable<WebSites>> GetAllWebSitesAsync();
        Task<WebSites> GetWebSiteByIdAsync(ObjectId id);
        Task<WebSites> AddWebSiteAsync(WebSites siteDto);
        Task<WebSites> UpdateWebSiteAsync(ObjectId id, WebSites siteDto);
        Task<bool> DeleteWebSiteAsync(string id);
        Task <WebSites> GetWebSiteBySiteNameAsync(string name);
    }
}

