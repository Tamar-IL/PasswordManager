using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DAL;
using DTO;
using Entities.models;
using IBL;
using IDAL;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;


namespace BL.NewFolder
{
    public class WebSitesBL : IWebSitesBL
    {
        private readonly ILogger<WebSitesBL> _logger;
        private readonly IWebSitesRepository _webSitesRepository;
        private readonly IMapper _mapper;

        //AutoMapper.MapperConfiguration configWebSiteConverter;
        public WebSitesBL(IWebSitesRepository webSitesRepository, ILogger<WebSitesBL> logger, IMapper mapper)
        {
            _logger = logger;
            _webSitesRepository = webSitesRepository;
            _mapper = mapper;
         
        }

        public async Task<IEnumerable<WebSitesDTO>> GetAllWebSitesAsync()
        {
            try
            {
                //Mapper mapper = new Mapper(configWebSiteConverter);

                var list = await _webSitesRepository.GetAllWebSitesAsync();
                List<WebSitesDTO> convertedList = new List<WebSitesDTO>();

                if (list.Count() == 0)
                    throw new Exception("there is no websites yet.");
                else
                {
                    foreach (var item in list)
                    {
                        convertedList.Add(_mapper.Map<WebSitesDTO>(item));
                    }
                }

                return convertedList;
            }
            catch (Exception ex)
            {
                throw new Exception("An error while retrieving passwords", ex);
            }
        }

        public async Task<WebSitesDTO> GetWebSitesByNameAsync(string siteName)
        {
            try
            {
                //var stringId = id.ToString();
                WebSites site = await _webSitesRepository.GetWebSiteBySiteNameAsync(siteName);
                if (site == null)
                {
                    return null;
                }

                return _mapper.Map<WebSitesDTO>(site);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the password .");
                throw new Exception($"An error occurred while retrieving the password.", ex);
            }
        }

        public async Task<WebSitesDTO> GetWebSiteByIdAsync(ObjectId id)
        {
            try
            {
                //var stringId = id.ToString();
                var site = await _webSitesRepository.GetWebSiteByIdAsync(id);
                if (site == null)
                {
                    return null;
                }

                return _mapper.Map<WebSitesDTO>(site);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the password with ID {id}.");
                throw new Exception($"An error occurred while retrieving the password with ID {id}.", ex);
            }
        }

        public async Task<WebSitesDTO> AddWebSiteAsync(WebSitesDTO siteDto)
        {
            try
            {
                var webSite = _mapper.Map<WebSites>(siteDto);
                await _webSitesRepository.AddWebSiteAsync(webSite);
                return siteDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the user.");
                throw;
            }
        }

        public async Task<WebSitesDTO> UpdateWebSiteAsync(ObjectId id, WebSitesDTO siteDto)
        {
            try
            {
                var webSites = _mapper.Map<WebSites>(siteDto);
                await _webSitesRepository.UpdateWebSiteAsync(id, webSites);
                return siteDto;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the password with ID {id}.");
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);
            }   
        }

        public async Task<bool> DeleteWebSiteAsync(ObjectId id)
        {
            try
            {
                string stringId = id.ToString();
                return await _webSitesRepository.DeleteWebSiteAsync(stringId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the password with ID {id}.");
                throw new Exception($"An error occurred while deleting the password with ID {id}.", ex);
            }
        }
    }
}
