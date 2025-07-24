using DTO;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IUsersBL
    {
        Task<IEnumerable<UsersDTO>> GetAllUsersAsync();
        Task<UsersDTO> GetUserByIdAsync(ObjectId id);
        Task<UsersDTO> GetUserByEmailAsync(string email);
        Task<UsersDTO> AddUserAsync(UsersDTO userDto);
        Task<UsersDTO> UpdateUserAsync(ObjectId id, UsersDTO userDto);
        Task<bool> DeleteUserAsync(ObjectId id);
        //Task<UsersDTO> Login(string email, string Password);
        Task<(UsersDTO user, string mfaCode)> Login(string email, string password); 
        bool VerifyMfaCode(string generatedCode, string inputCode);
        string GenerateTimeBasedMfaCode(string email);
        bool VerifyTimeBasedMfaCode(string email, string inputCode);
       




    }
}