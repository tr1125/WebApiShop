using DTOs;
using Entities;
//using WebApiShop.Controllers;

namespace Services
{
    public interface IUserService
    {
        Task<UserDTO> AddUserToFile(UserRequestDTO user);
        Task<List<UserDTO>> GetAllUsers();
        Task<UserDTO> GetUserById(int id);
        Task<UserDTO> Loginto(UserLoginDTO oldUser);
        Task<UserDTO> UpdateUserDetails(int id, UserRequestDTO userToUp);
    }
}