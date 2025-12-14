using Entities;
//using WebApiShop.Controllers;

namespace Services
{
    public interface IUserService
    {
        Task<User> AddUserToFile(User user);
        Task<List<User>> GetAllUsers();
        Task<User> GetUserById(int id);
        Task<User?> Loginto(ExistUser oldUser);
        Task<User> UpdateUserDetails(int id, User userToUp);
    }
}