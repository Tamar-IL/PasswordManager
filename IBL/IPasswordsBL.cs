using DTO;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IPasswordsBL
    {
            Task<IEnumerable<PasswordsDTO>> GetAllPasswordsAsync();
            Task<PasswordsDTO> GetPasswordByIdAsync(ObjectId id); 
            Task<PasswordsDTO> GetPasswordBySiteIdAsync(ObjectId id);
            Task<PasswordsDTO> AddPasswordAsync(string password);
            Task<PasswordsDTO> UpdatePasswordAsync(ObjectId id, PasswordsDTO passwordDto);
            Task<bool> DeletePasswordAsync(ObjectId id);
       

    }
}
