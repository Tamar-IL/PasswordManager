using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DTO;
using Entities;
using IDAL;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL
{
    public class PasswordsRepository : IPasswordsRepository
    {
        private readonly MongoDbService _dbService;
        private readonly IMapper _mapper;

        public PasswordsRepository(MongoDbService dbService, IMapper mapper)
        {
            _dbService = dbService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PasswordsDTO>> GetAllPasswordsAsync()
        {
            try
            {
                var passwords = await _dbService.GetCollection<PasswordsDTO>("Passwords").FindAsync(_ => true).Result.ToListAsync();
                return _mapper.Map<IEnumerable<PasswordsDTO>>(passwords);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while retrieving passwords.", ex);
            }
        }

        public async Task<PasswordsDTO> GetPasswordByIdAsync(int id)
        {
            try
            {
                var collection = _dbService.GetCollection<PasswordsDTO>("Passwords");
                return await collection.Find(p => p.PasswordId == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($" error occurred while retrieving the password with ID {id}.", ex);
            }
        }

        public async Task<PasswordsDTO> AddPasswordAsync(PasswordsDTO passwordDto)
        {
            try
            {

                passwordDto.Id = ObjectId.GenerateNewId().ToString(); // יצירת ערך Id חדש בפורמט הנכון
                var password = _mapper.Map<Passwords>(passwordDto);
                await _dbService.GetCollection<Passwords>("passwords").InsertOneAsync(password);
                return passwordDto;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while adding the password.", ex);
            }
        }

        public async Task<PasswordsDTO> UpdatePasswordAsync(int id, PasswordsDTO passwordDto)
        {
            try
            {
                var result = await _dbService.GetCollection<PasswordsDTO>("Passwords")
                    .ReplaceOneAsync(p => p.PasswordId == id, passwordDto);
                return result.IsAcknowledged ? passwordDto : null;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);
            }
        }

        public async Task<bool> DeletePasswordAsync(int id)
        {

            try
            {
                var result = await _dbService.GetCollection<PasswordsDTO>("Passwords")
                    .DeleteOneAsync(p => p.PasswordId == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while deleting the password with ID {id}.", ex);
            }
        }
    }
}
