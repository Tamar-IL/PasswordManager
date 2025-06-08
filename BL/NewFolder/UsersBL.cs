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
using System.Threading.Tasks;

namespace BL.NewFolder
{
    public class UsersBL : IUsersBL
    {
        private readonly ILogger<UsersBL> _logger;
        private readonly IUsersRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IRSAencryption _RSAencryption;

        public UsersBL(IRSAencryption RSAencryption, IUsersRepository userRepository, ILogger<UsersBL> logger, IMapper mapper)
        {
            _logger = logger;
            _userRepository = userRepository;
            _mapper = mapper;
            _RSAencryption = RSAencryption;
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

        public async Task<UsersDTO> GetUserByUserNameAsync(string UserName)
        {
            try
            {
                var user = await _userRepository.GetUserByUserNameAsync(UserName);
                if (user == null)
                {
                    throw new Exception($"User with userName {UserName} not found.");
                }
                return _mapper.Map<UsersDTO>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the user with username {UserName}.");
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
                byte[] encryptedPassword = _RSAencryption.Encrypt(userDto.Password, _RSAencryption.GetPublicKey());

                //byte[] encryptedPassword = _RSAencryption.Encrypt(pass, _RSAencryption.GetPublicKey());
                user.Password = encryptedPassword;
                //userDto.Password = Convert.ToBase64String(encryptedPassword);
                await _userRepository.AddUserAsync(user);
                userDto.Password = null;

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the user.");
                throw;
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
        public async Task<UsersDTO> Login(string email, string Password)
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
                            var userDto = _mapper.Map<UsersDTO>(user);
                            userDto.Password = null; // אל תחזיר סיסמה
                            return userDto;
                        }
                    }
                }
                return new UsersDTO();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                throw new Exception("error ", ex);
            }
        }




    }
}