using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDAL;
using MongoDB.Driver;
using AutoMapper;
using MongoDB.Bson;
using Entities.models;
using MongoDB.Bson;
 


namespace DAL
{
    public class WebSitesRepository : IWebSitesRepository
    {
        private readonly MongoDbService _dbService;
        private readonly IMapper _mapper;
        public WebSitesRepository(MongoDbService dbService, IMapper mapper)
        {
            _dbService = dbService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WebSites>> GetAllWebSitesAsync()
        {
            try
            {
                var webSites = await _dbService.GetCollection<WebSites>("WebSites").FindAsync(_ => true).Result.ToListAsync();
                return _mapper.Map<IEnumerable<WebSites>>(webSites);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with .", ex);
            }
        }
        public async Task<WebSites> GetWebSiteByIdAsync(ObjectId id)
        {
            try
            {
                string Id = id.ToString();
                var site =  _dbService.GetCollection<WebSites>("WebSites");
                return await site.Find(p => p.Id == id.ToString()).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while ", ex);
            }
        }
        public async Task<WebSites> GetWebSiteByUrlAsync(string url)
        {
            try
            {
                var site = _dbService.GetCollection<WebSites>("WebSites");
                return await site.Find(p => p.baseAddress == url).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while ", ex);
            }
        }
        public async Task<WebSites> AddWebSiteAsync(WebSites site)
        {
            try
            {
                if (string.IsNullOrEmpty(site.Id) || !ObjectId.TryParse(site.Id, out _))
                {
                    site.Id = ObjectId.GenerateNewId().ToString(); // יצירת ID חדש אם לא קיים
                }
                //var site = _mapper.Map<WebSitesDTO>(siteDto);
                await _dbService.GetCollection<WebSites>("WebSites").InsertOneAsync(site);
               
                return site;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password  .", ex);
            }
        }
        public async Task<WebSites> UpdateWebSiteAsync(ObjectId id, WebSites site)
        {
            try
            {
                string convertid = id.ToString();
                if (_dbService.GetDocumentByIdAsync<WebSites>("webSites", convertid) == null)
                {
                    throw new Exception($"webSite with ID {id} not found.");
                }
                else
                {
                    //var site = _mapper.Map<WebSites>(site);
                    var result = await _dbService.GetCollection<WebSites>("WebSites").ReplaceOneAsync(p => p.Id == convertid, site);
                    return result.IsAcknowledged ? site : null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);
            }
        }
        public async Task<bool> DeleteWebSiteAsync(string id)
        {
            try
            {
                //var stringId = id.ToString();
                var result = await _dbService.GetCollection<WebSites>("WebSites").DeleteOneAsync(p => p.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);
            }

        }
        public async Task<WebSites> GetWebSiteBySiteNameAsync(string name)

        {
            try
            {
                //string Id = id.ToString();
                var site = _dbService.GetCollection<WebSites>("WebSites");
                var result = await site.Find(p => p.Name == name).FirstOrDefaultAsync();
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while ", ex);
            }

        }

    }
}
