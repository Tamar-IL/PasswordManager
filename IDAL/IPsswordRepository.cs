using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace IDAL
{
    public interface IPasswordsRepository
    {
        Task<IEnumerable<PasswordsDTO>> GetAllPasswordsAsync();
        Task<PasswordsDTO> GetPasswordByIdAsync(int id);
        Task<PasswordsDTO> AddPasswordAsync(PasswordsDTO passwordDto);
        Task<PasswordsDTO> UpdatePasswordAsync(int id, PasswordsDTO passwordDto);
        Task<bool> DeletePasswordAsync(int id);
    }


}
