using AutoMapper;
using DTO;
using Entities.models;
using IDAL;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public class UsersRepository : IUsersRepository
    {
        private readonly MongoDbService _dbService;
        private readonly IMapper _mapper;
        private readonly MongoClient _client;

        public UsersRepository(MongoDbService dbService, IMapper mapper)
        {
            _dbService = dbService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            try{            
                var users = await _dbService.GetCollection<Users>("Users").FindAsync(_ => true).Result.ToListAsync();
                //var clusterDescription = _client.Cluster.Description;
                //if (clusterDescription.State == MongoDB.Driver.Core.Clusters.ClusterState.Disconnected)
                //{
                //    Console.WriteLine("/////////////////The cluster is disconnected. Check your connection.");
                //}
                return _mapper.Map<IEnumerable<Users>>(users);
            }
            catch (Exception ex) { 
                throw new Exception($"An error occurred while updating the password with .", ex);
            }
        }

        public async Task<Users> GetUserByIdAsync(ObjectId id)
        {
            try{
                string stringId = id.ToString();
                var collection = _dbService.GetCollection<Users>("Users");
                return await collection.Find(p => p.Id == id.ToString()).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);

            }
        }

        public async Task<Users> AddUserAsync(Users user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Id) || !ObjectId.TryParse(user.Id, out _))
                {
                    user.Id = ObjectId.GenerateNewId().ToString(); // יצירת ID חדש אם לא קיים
                }
                await _dbService.GetCollection<Users>("Users").InsertOneAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the user with .", ex);

            }
        }

        public async Task<Users> UpdateUserAsync(ObjectId id, Users user)
        {
            try
            {
                string stringId = id.ToString();
                if (_dbService.GetDocumentByIdAsync<Users>("Users", stringId) == null)
                {
                    throw new Exception($"user with ID {id} not found.");
                }
                else { 
                var result = await _dbService.GetCollection<Users>("Users")
                  .ReplaceOneAsync(p => p.Id == stringId, user);
                    return result.IsAcknowledged ? user : null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the user with ID {id}.", ex);

            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var result = await _dbService.GetCollection<UsersDTO>("Users").DeleteOneAsync(p => p.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the user with ID {id}.", ex);

            }
        }


    }
}