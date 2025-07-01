using AutoMapper;
using BL.RSAForMasterKay;
using DAL;
using DTO;
using Entities.models;
using IBL;
using IBL.RSAForMasterKey;
using IDAL;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BL.NewFolder
{
    public class UsersBL : IUsersBL
    {
        private readonly ILogger<UsersBL> _logger;
        private readonly IUsersRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IRSAencryption _RSAencryption;
        private readonly HttpClient _httpClient;


        public UsersBL(IRSAencryption RSAencryption, IUsersRepository userRepository, ILogger<UsersBL> logger, IMapper mapper, HttpClient httpClient)
        {
            _logger = logger;
            _userRepository = userRepository;
            _mapper = mapper;
            _RSAencryption = RSAencryption;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UsersDTO>> GetAllUsersAsync()
        {
            try
            {
                var list = await _userRepository.GetAllUsersAsync();
                List<UsersDTO> convertedList = new List<UsersDTO>();

                if (list.Count() == 0)
                    throw new Exception("there is no users yet.");
                else
                {
                    foreach (var item in list)
                    {
                        convertedList.Add(_mapper.Map<UsersDTO>(item));
                    }
                }
                return convertedList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users.");
                throw;
            }
        }
        public async Task<UsersDTO> GetUserByIdAsync(ObjectId id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    throw new Exception($"User with ID {id} not found.");
                }
                return _mapper.Map<UsersDTO>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the user with ID {id}.");
                throw;
            }
        }

        public async Task<UsersDTO> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmaileAsync(email);
                if (user == null)
                {
                    throw new Exception($"User with userName {email} not found.");
                }
                return _mapper.Map<UsersDTO>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the user with username {email}.");
                throw;
            }
        }

        public async Task<UsersDTO> AddUserAsync(UsersDTO userDto)
        {
            try
            {
                var user = _mapper.Map<Users>(userDto);
                var users = await _userRepository.GetAllUsersAsync();
                
                foreach (var User in users)
                {
                    if (User.Email == user.Email)
                        throw new Exception("email exist yet");
                }
                var code = GenerateTimeBasedMfaCode(user.Email);
                var data = new { Email = userDto.Email, Code = code };
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var verifyResponse = await _httpClient.PostAsync("https://hook.us2.make.com/bvvrpmw6v42rekqcn0b2248srwplt9mw",content);
                if (!verifyResponse.IsSuccessStatusCode)
                {
                    byte[] encryptedPassword = _RSAencryption.Encrypt(userDto.Password, _RSAencryption.GetPublicKey());
                    user.Password = encryptedPassword;
                    await _userRepository.AddUserAsync(user);
                    userDto.Password = null;

                    return userDto;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the user.");
                throw;
            }
        }
        //public int sixDigitMFA()
        //{
        //    using var rng = RandomNumberGenerator.Create();
        //    byte[] bytes = new byte[4];
        //    int result;

        //    do
        //    {
        //        rng.GetBytes(bytes);
        //        result = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
        //        result = result % 900000 + 100000; 
        //    }
        //    while (result > 999999 || result < 100000);

        //    return result;
        //}

        public string GenerateTimeBasedMfaCode(string email)
        {
            // 5-minute time windows
            var timeWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 300;

            // Secret key - בפרודקציה זה צריך להיות מהקונפיגורציה
            var secretKey = "your-secret-mfa-key-here";

            // Create input for hash
            var input = $"{email}:{timeWindow}:{secretKey}";

            // Generate hash
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert to 6-digit number
                var code = BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
                code = code % 1000000;

                // Ensure 6 digits with leading zeros
                return code.ToString("D6");
            }
        }

        public async Task<UsersDTO> UpdateUserAsync(ObjectId id, UsersDTO userDto)
        {
            try
            {
                var user = _mapper.Map<Users>(userDto);
                var updatedUser = await _userRepository.UpdateUserAsync(id, user);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the user with ID {id}.");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(ObjectId id)
        {
            try
            {
                string stringId = id.ToString();
                return await _userRepository.DeleteUserAsync(stringId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the user with ID {id}.");
                throw;
            }
        }
        public async Task<(UsersDTO user, string mfaCode)> Login(string email, string Password)
        {
            try
            {
                var allUsers = await _userRepository.GetAllUsersAsync();             

                foreach (Users user in allUsers)
                {
                    if (user.Email == email)
                    {

                        string decPas = _RSAencryption.Decrypt(user.Password, _RSAencryption.GetPrivateKayFromSecureStorge());
                        Console.WriteLine($"Decrypted: '{decPas}'");
                        Console.WriteLine($"Input: '{Password}'");
                        if (decPas == Password)
                        {
                            
                            //var mfaCode = sixDigitMFA();
                            var mfaCode = GenerateTimeBasedMfaCode(user.Email);

                            
                            var data = new { email = email, code = mfaCode };
                            var json = JsonSerializer.Serialize(data);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            await _httpClient.PostAsync("https://hook.us2.make.com/bvvrpmw6v42rekqcn0b2248srwplt9mw", content);

                            var userDto = _mapper.Map<UsersDTO>(user);
                            userDto.Password = null;


                            //return (userDto, mfaCode.ToString());
                            return (userDto, mfaCode);

                            //var userDto = _mapper.Map<UsersDTO>(user);
                            //userDto.Password = null; // אל תחזיר סיסמה
                            //return userDto;
                        }
                    }
                }
                //return new UsersDTO();
                return (new UsersDTO(), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                throw new Exception("error ", ex);
            }
        }

        public bool VerifyMfaCode(string generatedCode, string inputCode)
        {
            return generatedCode == inputCode;
        }
        public bool VerifyTimeBasedMfaCode(string email, string inputCode)
        {
            // בדוק את החלון הנוכחי
            var currentCode = GenerateTimeBasedMfaCode(email);
            if (currentCode == inputCode)
                return true;

            // בדוק את החלון הקודם (למקרה של delay קטן)
            var timeWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 300 - 1;
            var secretKey = "your-secret-mfa-key-here";
            var input = $"{email}:{timeWindow}:{secretKey}";

            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                var code = BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
                code = code % 1000000;
                var previousCode = code.ToString("D6");

                return previousCode == inputCode;
            }
        }

    }
}