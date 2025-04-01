using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDAL;
using MongoDB.Driver;
using AutoMapper;
using Entities;
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


        public async Task<IEnumerable<WebSitesDTO>> GetAllWebSitesAsync()
        {
            try
            {
                var webSites = await _dbService.GetCollection<WebSites>("WebSites").FindAsync(_ => true).Result.ToListAsync();
                return _mapper.Map<IEnumerable<WebSitesDTO>>(webSites);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with .", ex);

            }
        }
        public async Task<WebSitesDTO> GetWebSiteByIdAsync(int id)
        {
            try
            {
                var site = await _dbService.GetCollection<WebSites>("WebSites").FindAsync(p => p.SiteId == id).Result.FirstOrDefaultAsync();
                return _mapper.Map<WebSitesDTO>(site);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);

            }

        }
        public async Task<WebSitesDTO> AddWebSiteAsync(WebSitesDTO siteDto)
        {
            try
            {
                var site = _mapper.Map<WebSites>(siteDto);
                await _dbService.GetCollection<WebSites>("WebSites").InsertOneAsync(site);
                return siteDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password  .", ex);

            }



        }
        public async Task<WebSitesDTO> UpdateWebSiteAsync(int id, WebSitesDTO siteDto)
        {
            try
            {

                var site = _mapper.Map<WebSites>(siteDto);
                var result = await _dbService.GetCollection<WebSites>("WebSites").ReplaceOneAsync(p => p.SiteId == id, site);
                return result.IsAcknowledged ? siteDto : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);

            }

        }
        public async Task<bool> DeleteWebSiteAsync(int id)
        {
            try
            {
                var result = await _dbService.GetCollection<WebSitesDTO>("websites").DeleteOneAsync(p => p.SiteId == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);

            }
        }
    }
}
