using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Entities.models;
using MongoDB.Bson;

namespace IDAL
{
    public interface IPasswordsRepository
    {
        Task<IEnumerable<Passwords>> GetAllPasswordsAsync();
        Task<Passwords> GetPasswordByIdAsync(ObjectId id);
        Task<Passwords> GetPasswordBySiteIdAsync(ObjectId id);
        Task<Passwords> UpdatePasswordAsync(ObjectId id, Passwords password);
        Task<bool> DeletePasswordAsync(string id);
        Task<Passwords> AddPasswordAsync(Passwords password);

        Task<bool> PasswordExistsForUserAndSiteAsync(string userId, string siteId);

    }

}
