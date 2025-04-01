using DTO;
using IBL;
using IDAL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BL
{
    public class Users : IUsers
    {
        private readonly IUsersRepository _userRepository;

        public Users(IUsersRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UsersDTO>> GetAllUsersAsync()
        {
            Console.WriteLine("BL");
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<UsersDTO> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<UsersDTO> AddUserAsync(UsersDTO userDto)
        {
            return await _userRepository.AddUserAsync(userDto);
        }

        public async Task<UsersDTO> UpdateUserAsync(int id, UsersDTO userDto)
        {
            return await _userRepository.UpdateUserAsync(id, userDto);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }
    }
}