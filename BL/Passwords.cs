using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using IBL;
using IDAL;

namespace BL
{
    public class Passwords : IPasswords
    {
        private readonly IPasswordsRepository _passwordsRepository;

        public Passwords(IPasswordsRepository passwordsRepository)
        {
            _passwordsRepository = passwordsRepository;
        }

        public async Task<IEnumerable<PasswordsDTO>> GetAllPasswordsAsync()
        {
            return await _passwordsRepository.GetAllPasswordsAsync();
        }

        public async Task<PasswordsDTO> GetPasswordByIdAsync(int id)
        {
            return await _passwordsRepository.GetPasswordByIdAsync(id);
        }

        public async Task<PasswordsDTO> AddPasswordAsync(PasswordsDTO passwordDto)
        {
            return await _passwordsRepository.AddPasswordAsync(passwordDto);
        }

        public async Task<PasswordsDTO> UpdatePasswordAsync(int id, PasswordsDTO passwordDto)
        {
            return await _passwordsRepository.UpdatePasswordAsync(id, passwordDto);
        }

        public async Task<bool> DeletePasswordAsync(int id)
        {
            return await _passwordsRepository.DeletePasswordAsync(id);
        }
    }

}
