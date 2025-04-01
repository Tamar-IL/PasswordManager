using AutoMapper;
using DTO;
using Entities;
using IDAL;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public class UsersRepository : IUsersRepository
    {
        private readonly MongoDbService _dbService;
        private readonly IMapper _mapper;

        public UsersRepository(MongoDbService dbService, IMapper mapper)
        {
            _dbService = dbService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UsersDTO>> GetAllUsersAsync()
        {
            try
            {
                var users = await _dbService.GetCollection<Users>("Users").FindAsync(_ => true).Result.ToListAsync();
                return _mapper.Map<IEnumerable<UsersDTO>>(users);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with .", ex);

            }
        }

        public async Task<UsersDTO> GetUserByIdAsync(int id)
        {
            try
            {

                var user = await _dbService.GetCollection<Users>("Users").FindAsync(p => p.UserId == id).Result.FirstOrDefaultAsync();
                return _mapper.Map<UsersDTO>(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);

            }
        }

        public async Task<UsersDTO> AddUserAsync(UsersDTO userDto)
        {
            try
            {
                var user = _mapper.Map<Users>(userDto);
                await _dbService.GetCollection<Users>("Users").InsertOneAsync(user);
                return userDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with .", ex);

            }
        }

        public async Task<UsersDTO> UpdateUserAsync(int id, UsersDTO userDto)
        {
            try
            {
                var user = _mapper.Map<Users>(userDto);
                var result = await _dbService.GetCollection<Users>("Users").ReplaceOneAsync(p => p.UserId == id, user);
                return result.IsAcknowledged ? userDto : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);

            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var result = await _dbService.GetCollection<Users>("Users").DeleteOneAsync(p => p.UserId == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);

            }
        }
    }
}