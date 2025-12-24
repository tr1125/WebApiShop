using Entities;
//using WebApiShop.Controllers;

namespace Repositories
{
    public interface IUserRepository
    {
        Task<User> AddUserToFile(User user);
        Task<List<User>> GetAllUsers();
        Task<User> GetUserById(int id);
        Task<User?> Loginto(User oldUser);
        Task UpdateUserDetails(int id, User userToUp);
    }
}