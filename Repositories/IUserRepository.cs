using Entities;
using WebApiShop.Controllers;

namespace Repositories
{
    public interface IUserRepository
    {
        Task<User> AddUserToFile(User user);
        IEnumerable<User> GetAllUsers();
        Task<User> GetUserById(int id);
        Task<User?> Loginto(ExistUser oldUser);
        Task UpdateUserDetails(int id, User userToUp);
    }
}