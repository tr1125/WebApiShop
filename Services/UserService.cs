using Entities;
using Repositories;
using WebApiShop.Controllers;
using Zxcvbn;
namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public Users GetUserById(int id) { return _repository.GetUserById(id); }

        public Users AddUserToFile(Users user)
        {
            Password password=PasswordHardness(user.Password);
            if (password.Level < 3) return null;
            return _repository.addUserToFile(user);
        }

        public Users Loginto(ExistUser oldUser) { return _repository.Loginto(oldUser); }

        public List<Users> GetAllUsers() { return _repository.GetAllUsers(); }

        public Users UpdateUserDetails(int id, Users userToUp) {

            Password password = PasswordHardness(userToUp.Password);
            if (password.Level <3) return null;
            _repository.UpdateUserDetails(id, userToUp);
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
