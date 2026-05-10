using DTOs;
using Entities;
//using WebApiShop.Controllers;

namespace Services
{
    public interface IUserService
    {
        Task<(UserDTO User, string Token)?> AddUserToFile(UserRequestDTO user);

        Task<List<UserDTO>> GetAllUsers();
        Task<UserDTO> GetUserById(int id);
        Task<(UserDTO User, string Token)?> Loginto(UserLoginDTO oldUser);
        Task<UserDTO> UpdateUserDetails(int id, UserRequestDTO userToUp);
        Task<bool> PromoteToAdmin(int id);

        string GenerateToken(UserDTO user);

    }
}