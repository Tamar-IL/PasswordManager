using DTO;
using Entities.models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IUsersRepository
    {
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users> GetUserByIdAsync(ObjectId id);
        Task<Users> GetUserByUserNameAsync(string userName);
        Task<Users> AddUserAsync(Users user);
        Task<Users> UpdateUserAsync(ObjectId id, Users user);
        Task<bool> DeleteUserAsync(string id);
    }
}