using DTO;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBL
{
    public interface IUsersBL
    {
        Task<IEnumerable<UsersDTO>> GetAllUsersAsync();
        Task<UsersDTO> GetUserByIdAsync(ObjectId id);
        Task<UsersDTO> AddUserAsync(UsersDTO userDto);
        Task<UsersDTO> UpdateUserAsync(ObjectId id, UsersDTO userDto);
        Task<bool> DeleteUserAsync(ObjectId id);
    }
}