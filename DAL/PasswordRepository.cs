using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using DTO;
using Entities.models;
using IDAL;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpCompress.Common;

namespace DAL
{

    //    שכבה מקבל    מחזיר
    //Controller  DTO DTO
    //Service(BL)    DTO DTO
    //Repository(DAL)    Entity Entit
    public class PasswordsRepository : IPasswordsRepository
    {
        private readonly MongoDbService _dbService;
        private readonly IMapper _mapper;
        private readonly MongoClient _client;
        public PasswordsRepository(MongoDbService dbService, IMapper mapper)
        {
            _dbService = dbService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Passwords>> GetAllPasswordsAsync()
        {
            try
            {
                Console.WriteLine("pasDal");
                var passwords = await _dbService.GetCollection<Passwords>("passwords").FindAsync(_ => true).Result.ToListAsync();

                //var clusterDescription = _client.Cluster.Description;
                //if (clusterDescription.State == MongoDB.Driver.Core.Clusters.ClusterState.Disconnected)
                //{
                //    Console.WriteLine("/////////////////The cluster is disconnected. Check your connection.");
                //}  
                return _mapper.Map<IEnumerable<Passwords>>(passwords);

            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while retrieving passwords.", ex);
            }
        }

        public async Task<Passwords> GetPasswordByIdAsync(ObjectId id)
        {
            try
            {
                string Id = id.ToString();
                var collection = _dbService.GetCollection<Passwords>("passwords");
                return await collection.Find(p => p.Id == id.ToString()).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($" error occurred while retrieving the password with ID {id}.", ex);
            }
        }
        public async Task<Passwords> GetPasswordBySiteIdAsync(ObjectId id)
        {
            try
            {
                string Id = id.ToString();
                var collection = _dbService.GetCollection<Passwords>("passwords");

                return await collection.Find(p => p.SiteId == Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($" error occurred while retrieving the password with ID {id}.", ex);
            }
        }


        public async Task<Passwords> AddPasswordAsync(Passwords password)
        {
            try
            {
                if (string.IsNullOrEmpty(password.Id) || !ObjectId.TryParse(password.Id, out _))
                {
                    password.Id = ObjectId.GenerateNewId().ToString(); // יצירת ID חדש אם לא קיים
                }

                await _dbService.GetCollection<Passwords>("passwords").InsertOneAsync(password);
                return password;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the password.", ex);
            }
        }

        //public async Task<Passwords> AddPasswordAsync(Passwords password)
        //{
        //    try
        //    {

        //        PasswordsDTO passwordDto = new PasswordsDTO
        //        {
        //            Password = password.Password,
        //            Id = ObjectId.GenerateNewId().ToString(), // יצירת ID חדש
        //            DateReg = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        //            LastDateUse = DateTime.UtcNow.ToString("yyyy-MM-dd")
        //        };

        //        var passwordEntity = _mapper.Map<Passwords>(passwordDto);
        //        //object id = ObjectId.Parse(password.Id);
        //        //id = ObjectId.GenerateNewId(); // יצירת ערך Id חדש בפורמט הנכון
        //        await _dbService.GetCollection<Passwords>("passwords").InsertOneAsync(password);
        //        return password;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        throw new Exception("An error occurred while adding the password.", ex);
        //    }
        //}

        public async Task<Passwords> UpdatePasswordAsync(ObjectId id, Passwords password)
        {
            try
            {
                //ObjectId objectId = ObjectId.Parse(id);
                //password.Id = objectId.ToString(); // עדכון ה-Id של האובייקט

                string convertid = id.ToString();
                if (_dbService.GetDocumentByIdAsync<Passwords>("passwords", convertid) == null)
                {
                    throw new Exception($"Password with ID {id} not found.");
                }
                else
                {
                    var result = await _dbService.GetCollection<Passwords>("passwords")
                        .ReplaceOneAsync(p => p.Id == convertid, password);
                    return result.IsAcknowledged ? password : null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the password with ID {id}.", ex);
            }
        }


        public async Task<bool> DeletePasswordAsync(string id)
        {
            try
            {
                //var objectId = ObjectId.Parse(id); // Parse the string ID to ObjectId
                var result = await _dbService.GetCollection<Passwords>("passwords")
                    .DeleteOneAsync(p => p.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"An error occurred while deleting the password with ID {id}.", ex);
            }
        }
        //public async Task<PasswordsDTO> UpdatePasswordAsync(string id, PasswordsDTO dto)
        //{
        //    var entity = _mapper.Map<Passwords>(dto);
        //    var updatedEntity = await _passwordsRepository.UpdatePasswordAsync(id, entity);
        //    return _mapper.Map<PasswordsDTO>(updatedEntity);
        //}
    }
}
