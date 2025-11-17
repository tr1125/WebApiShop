using Entities;
using WebApiShop.Controllers;

namespace Services
{
    public interface IUserService
    {
        Users AddUserToFile(Users user);
        List<Users> GetAllUsers();
        Users GetUserById(int id);
        Users Loginto(ExistUser oldUser);
        Password PasswordHardness(string password);
        Users UpdateUserDetails(int id, Users userToUp);
    }
}