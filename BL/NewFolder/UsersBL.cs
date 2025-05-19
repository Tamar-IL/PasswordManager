using AutoMapper;
using DAL;
using DTO;
using Entities.models;
using IBL;
using IDAL;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BL.NewFolder
{
    public class UsersBL : IUsersBL
    {
        private readonly ILogger<UsersBL> _logger;
        private readonly IUsersRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersBL(IUsersRepository userRepository, ILogger<UsersBL> logger, IMapper mapper)
        {
            _logger = logger;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UsersDTO>> GetAllUsersAsync()
        {
            try
            {
                var list = await _userRepository.GetAllUsersAsync();
                List<UsersDTO> convertedList = new List<UsersDTO>();

                if (list.Count() == 0)
                    throw new Exception("there is no passwords yet.");

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

        public async Task<UsersDTO> AddUserAsync(UsersDTO userDto)
        {
            try
            {
                var user = _mapper.Map<Users>(userDto);
                await _userRepository.AddUserAsync(user);
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
    }
}