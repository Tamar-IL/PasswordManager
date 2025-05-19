using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DnsClient.Internal;
using DTO;
using Entities.models;
using IBL;
using IDAL;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace BL.NewFolder
{
    public class PasswordsBL : IPasswordsBL
    {
        private readonly ILogger<PasswordsBL> _logger;
        private readonly IPasswordsRepository _passwordsRepository;
        private readonly IMapper _mapper;

        MapperConfiguration configPasswordConverter;
        public PasswordsBL(IPasswordsRepository passwordsRepository, ILogger<PasswordsBL> logger, IMapper mapper)
        {
            _logger = logger;
            _passwordsRepository = passwordsRepository;
            _mapper = mapper;

            //configPasswordConverter = new AutoMapper.MapperConfiguration(a =>
            //         a.CreateMap<Passwords, PasswordsDTO>()
            //         .ForMember(x => x.Id, s => s.MapFrom(p => p.Id))
            //         // .ForMember(x => x.CustCity, s => s.MapFrom(p =>int.Parse( p.CustCity) ))
            //         .ReverseMap()
            //         .ForMember(x => x.Id, s => s.MapFrom(p => p.Id))
            //         // .ForMember(x => x.CustCity, s => s.MapFrom(p =>  p.CustCity.ToString() ))
            //         );
        }

        public async Task<IEnumerable<PasswordsDTO>> GetAllPasswordsAsync()
        {
            try
            {
                Console.WriteLine("PasBL");
                var list = await _passwordsRepository.GetAllPasswordsAsync();
                List<PasswordsDTO> convertedList = new List<PasswordsDTO>();

                if (list.Count() == 0)
                    throw new Exception("there is no passwords yet.");

                else
                {
                    foreach (var item in list)
                    {
                        convertedList.Add(_mapper.Map<PasswordsDTO>(item));
                    }
                }

                return convertedList;

            }
            catch (Exception ex)
            {
                throw new Exception("An error while retrieving passwords", ex);
            }
        }

        public async Task<PasswordsDTO> GetPasswordByIdAsync(ObjectId id)
        {
            try
            {
                //var stringId = id.ToString();
                var password = await _passwordsRepository.GetPasswordByIdAsync(id);
                if (password == null)
                {
                    return null;
                }

                return _mapper.Map<PasswordsDTO>(password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the password with ID {id}.");
                throw new Exception($"An error occurred while retrieving the password with ID {id}.", ex);
            }
        }
        public async Task<PasswordsDTO> GetPasswordBySiteIdAsync(ObjectId id)
        {
            try
            {
                //var stringId = id.ToString();
                var password = await _passwordsRepository.GetPasswordBySiteIdAsync(id);
                if (password == null)
                {
                    return null;
                }

                return _mapper.Map<PasswordsDTO>(password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the password with ID {id}.");
                throw new Exception($"An error occurred while retrieving the password with ID {id}.", ex);
            }
        }

        public async Task<PasswordsDTO> AddPasswordAsync(string password)
        {
            try
            {
                PasswordsDTO passwordDto = new PasswordsDTO
                {
                    Password = password,
                    Id = ObjectId.GenerateNewId().ToString(), // יצירת ID חדש
                    DateReg = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    LastDateUse = DateTime.UtcNow.ToString("yyyy-MM-dd")
                };

                var passwordEntity = _mapper.Map<Passwords>(passwordDto);
                await _passwordsRepository.AddPasswordAsync(passwordEntity);

                return passwordDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the password.");
                throw new Exception("An error occurred while adding the password.", ex);
            }
        }


        public async Task<PasswordsDTO> UpdatePasswordAsync(ObjectId id, PasswordsDTO password)
        {
            try
            {
                //var stringId = id.ToString();
                //Mapper mapper = new Mapper(configPasswordConverter);
                var pas = _mapper.Map<Passwords>(password);
                await _passwordsRepository.UpdatePasswordAsync(id,pas);
                return password;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the password with ID {id}.");
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);
            }
        }

        public async Task<bool> DeletePasswordAsync(ObjectId id)
        {
            try
            {
                string stringId = id.ToString();
                return await _passwordsRepository.DeletePasswordAsync(stringId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the password with ID {id}.");
                throw new Exception($"An error occurred while deleting the password with ID {id}.", ex);
            }
        }
        //public async Task<PasswordsDTO> AddPasswordWithSiteAsync(string password, string siteName, string siteBaseAddress, string userId)
        //{
        //    try
        //    {
        //        // בדוק אם האתר כבר קיים
        //        var existingSite = await _webSitesRepository.GetSiteByNameAsync(siteName);
        //        string siteId;

        //        if (existingSite != null)
        //        {
        //            // אם האתר קיים, השתמש ב-ID שלו
        //            siteId = existingSite.Id;
        //        }
        //        else
        //        {
        //            // אם האתר לא קיים, צור אתר חדש ושמור אותו
        //            var newSite = new WebSites
        //            {
        //                Id = ObjectId.GenerateNewId().ToString(),
        //                Name = siteName,
        //                BaseAddress = siteBaseAddress
        //            };
        //            await _webSitesRepository.AddWebSiteAsync(newSite);
        //            siteId = newSite.Id;
        //        }

        //        // צור את הסיסמא החדשה
        //        var passwordDto = new PasswordsDTO
        //        {
        //            Id = ObjectId.GenerateNewId().ToString(),
        //            Password = password,
        //            UserId = userId,
        //            SiteId = siteId,
        //            DateReg = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        //            LastDateUse = DateTime.UtcNow.ToString("yyyy-MM-dd")
        //        };

        //        // שמור את הסיסמא
        //        var passwordEntity = _mapper.Map<Passwords>(passwordDto);
        //        await _passwordsRepository.AddPasswordAsync(passwordEntity);

        //        return passwordDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while adding the password with site.");
        //        throw new Exception("An error occurred while adding the password with site.", ex);
        //    }
        //}

    }

}
