using WebApiShop.Controllers;

namespace Repositories
{
    public interface IUserRepository
    {



        Users addUserToFile(Users user);
        List<Users> GetAllUsers();
        Users GetUserById(int id);
        Users Loginto(ExistUser oldUser);
        void UpdateUserDetails(int id, Users userToUp);
    }
}