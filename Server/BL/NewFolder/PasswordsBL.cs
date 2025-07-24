using AutoMapper;
using DnsClient.Internal;
using DTO;
using Entities.models;
using IBL;
using IBL.RSAForMasterKey;
using IDAL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MyProject.Common;
using System.Text;


namespace BL.NewFolder
{
    public class PasswordsBL : IPasswordsBL
    {

        readonly ILogger<PasswordsBL> _logger;
        private readonly IPasswordsRepository _passwordsRepository;
        private readonly IUsersBL _userBL;
        private readonly IMapper _mapper;
        private readonly IWebSitesBL _webSite;
        private readonly MySetting _mySetting;
        private readonly IEncryptionProcess _encryptionProcess;
        private readonly IDecryptionProcess _decryptionProcess;
        private readonly IRSAencryption _RSAencryption;
        //private readonly IAESEncryptionBL _AESEncryption;



        MapperConfiguration configPasswordConverter;
        public PasswordsBL(IRSAencryption RSAencryption, IWebSitesBL webSite, IUsersBL userBL, IPasswordsRepository passwordsRepository, ILogger<PasswordsBL> logger, IMapper mapper, IOptions<MySetting> mySettingOptions, IEncryptionProcess encryptionProcess, IDecryptionProcess decryptionProcess, IAESEncryptionBL aESEncryption)
        {
            _webSite = webSite;
            _logger = logger;
            _userBL = userBL;
            _passwordsRepository = passwordsRepository;
            _mapper = mapper;
            _mySetting = mySettingOptions.Value;
            _encryptionProcess = encryptionProcess;
            _RSAencryption = RSAencryption;
            _decryptionProcess = decryptionProcess;
            //_AESEncryption = aESEncryption;

            _encryptionProcess = encryptionProcess;
        }

        public async Task<IEnumerable<PasswordsDTO>> GetAllPasswordsForUserByUserIdAsync(string id)
        {
            try
            {
                var list = await _passwordsRepository.GetAllPasswordsAsync();
                List<PasswordsDTO> convertedList = new List<PasswordsDTO>();
                foreach (var item1 in list)
                {
                    if (item1.UserId == id)
                    {
                        {
                            //עדיף לא להחזיר את כל הסיסמאות מפוענחות רק סיסמה שהמשתמש ילחץ על העין אז זה ישלח בקשה לפענוח.

                            //פענוח VP
                            //byte[] bytesVP = Convert.FromBase64String(item1.VP);

                            //string vp = _RSAencryption.Decrypt(bytesVP);

                            ////חחזרת VP לצורת רשימה
                            //List<int> vectorOfPositions = vp.Split(',').Select(int.Parse).ToList();

                            ////פענוח הסיסמא באמצעות הVP
                            //string password = _decryptionProcess.Decrypt(item1.Password, vectorOfPositions);


                            //item1.Password = _RSAencryption.Encrypt(item1.Password.ToString(),publicKey).Select(c => (int)c).ToArray();
                        }
                        convertedList.Add(_mapper.Map<PasswordsDTO>(item1));

                    }

                }
                return convertedList;

            }
            catch (Exception ex)
            {
                throw new Exception("An error while retrieving passwords", ex);
            }
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

        public async Task<string> GetPasswordByIdAsync(ObjectId id, string publicKey)
        {
            try
            {
                //var stringId = id.ToString();
                var password = await _passwordsRepository.GetPasswordByIdAsync(id);
                if (password != null)
                {
                    byte[] bytesVP = Convert.FromBase64String(password.VP);
                    string vp = _RSAencryption.Decrypt(bytesVP);

                    //חחזרת VP לצורת רשימה
                    List<int> vectorOfPositions = vp.Split(',').Select(int.Parse).ToList();

                    //פענוח הסיסמא באמצעות הVP
                    string passwordEnc = _decryptionProcess.Decrypt(password.Password, vectorOfPositions);


                    //password.Password = _RSAencryption.Encrypt(passwordEnc, publicKey).Select(c => (int)c).ToArray();
                    byte[] encryorPass = _RSAencryption.Encrypt(passwordEnc, publicKey);
                    string base64Encrypted = Convert.ToBase64String(encryorPass);
                    return base64Encrypted;
                    //return _mapper.Map<PasswordsDTO>( password);
                }
                else
                {
                    return null;  
                }
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

        public async Task<PasswordsDTO> AddPasswordAsync(PasswordsDTO password, string url)
        {
            try
            {
                string actualPassword = password.Password;
                byte[] encryptedBytes = Convert.FromBase64String(actualPassword);
                actualPassword = _RSAencryption.Decrypt(encryptedBytes, _RSAencryption.GetPrivateKayFromSecureStorge());
                Console.WriteLine(actualPassword);
                password.Id = ObjectId.GenerateNewId().ToString();
                password.DateReg = DateTime.UtcNow.ToString("yyyy-MM-dd");
                password.LastDateUse = DateTime.UtcNow.ToString("yyyy-MM-dd");
                password.Password = actualPassword;

                // הצפנה
                var (encryptPass, VP) = _encryptionProcess.Encrypt(password.Password);
                //encrypt vp by rsa
                if (VP != null)
                {
                    // המרת List<int> ל-string לשמירה זמנית
                    string vpString = string.Join(",", VP);

                    // המרת string ל-byte[] להצפנה RSA
                    byte[] vpBytes = Encoding.UTF8.GetBytes(vpString);

                    // הצפנת VP עם RSA
                    byte[] encryptedVP = _RSAencryption.Encrypt(vpString, _RSAencryption.GetPublicKey());

                    // המרת byte[] ל-Base64 string לשמירה ב-DB
                    password.VP = Convert.ToBase64String(encryptedVP);
                }
                else
                {
                    _logger.LogWarning("VP is null or empty after encryption");
                    password.VP = "";
                }

                WebSitesDTO newSite = new WebSitesDTO();
                newSite.baseAddress = _webSite.splitUrl(url);
                newSite.Id = ObjectId.GenerateNewId().ToString();
                var web = await _webSite.AddWebSiteAsync(newSite);
                if (web.Id != null)
                    password.SiteId = web.Id;
                else
                    password.SiteId = newSite.Id;

                //  בדיקת כפילויות
                var passwordExists = await _passwordsRepository.PasswordExistsForUserAndSiteAsync(password.UserId, password.SiteId);
                if (passwordExists)
                {
                    throw new InvalidOperationException($"A password already exists for this user and website. Please update the existing password instead of creating a new one.");
                }

                // יצירת Entity
                var passwordEntity = _mapper.Map<Passwords>(password);
                passwordEntity.Password = encryptPass;

                _logger.LogInformation($"About to save: ID={passwordEntity.Id}, SiteId={passwordEntity.SiteId}, VP='{passwordEntity.VP}'");

                // שמירה
                var result = await _passwordsRepository.AddPasswordAsync(passwordEntity);
                _logger.LogInformation($"Save result: {result?.Id}");

                return password;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddPasswordAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<PasswordsDTO> UpdatePasswordAsync(ObjectId id, PasswordsDTO password)
        {
            try
            {
                //var stringId = id.ToString();
                //Mapper mapper = new Mapper(configPasswordConverter);
                var pas = _mapper.Map<Passwords>(password);
                await _passwordsRepository.UpdatePasswordAsync(id, pas);
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
