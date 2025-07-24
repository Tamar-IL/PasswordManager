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
            Task <string>  GetPasswordByIdAsync(ObjectId id,string publicke); 
            Task<PasswordsDTO> GetPasswordBySiteIdAsync(ObjectId id);
            Task<PasswordsDTO> AddPasswordAsync(PasswordsDTO password, string url);
            Task<PasswordsDTO> UpdatePasswordAsync(ObjectId id, PasswordsDTO passwordDto);
            Task<bool> DeletePasswordAsync(ObjectId id);
            Task<IEnumerable<PasswordsDTO>> GetAllPasswordsForUserByUserIdAsync(string id);

    }
}
