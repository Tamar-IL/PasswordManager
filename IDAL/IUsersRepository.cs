using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IUsersRepository
    {
        Task<IEnumerable<UsersDTO>> GetAllUsersAsync();
        Task<UsersDTO> GetUserByIdAsync(int id);
        Task<UsersDTO> AddUserAsync(UsersDTO userDto);
        Task<UsersDTO> UpdateUserAsync(int id, UsersDTO userDto);
        Task<bool> DeleteUserAsync(int id);
    }
}