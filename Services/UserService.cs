using Entities;
using Repositories;
using WebApiShop.Controllers;
using Zxcvbn;
namespace Services
{
    public class UserService// : IUserService
    {
        UserRepository repository = new UserRepository();

        public Users GetUserById(int id) { return repository.GetUserById(id); }

        public Users AddUserToFile(Users user)
        {
            Password password=PasswordHardness(user.Password);
            if (password.Level < 3) return null;
            return repository.addUserToFile(user);
        }

        public Users Loginto(ExistUser oldUser) { return repository.Loginto(oldUser); }

        public List<Users> GetAllUsers() { return repository.GetAllUsers(); }

        public Users UpdateUserDetails(int id, Users userToUp) {

            Password password = PasswordHardness(userToUp.Password);
            if (password.Level <3) return null;
            repository.UpdateUserDetails(id, userToUp);
            return userToUp;
        }

        public Password PasswordHardness(string password)
        {
            Password password1 = new Password();
            password1.Name = password;
            password1.Level = Zxcvbn.Core.EvaluatePassword(password).Score;
            return password1;
        }
    }
}
