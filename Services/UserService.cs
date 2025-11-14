using Entities;
using Repositories;
using WebApiShop.Controllers;
using Zxcvbn;
namespace Services
{
    public class UserService
    {
        UserRepository repository=new UserRepository();

        public Users GetUserById(int id) {  return repository.GetUserById(id); }

        public Users AddUserToFile(Users user) {return repository.addUserToFile(user);
        }

        public Users Loginto(ExistUser oldUser) { return repository.Loginto(oldUser); }

        public void UpdateUserDetails(int id, Users userToUp) {  repository.UpdateUserDetails(id, userToUp); }

        public Password PasswordHardness(string password)
        {
            Password password1 = new Password();
            password1.Name = password;
            password1.Level = Zxcvbn.Core.EvaluatePassword(password).Score;
            return password1;
        }
    }
}
