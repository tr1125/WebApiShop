using Entities;
using Repositories;
using System.Threading.Tasks;
using WebApiShop.Controllers;
using Zxcvbn;


namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordService _passwordService;
        public UserService(IUserRepository repository, IPasswordService passwordService)
        {
            _repository = repository;
            _passwordService = passwordService;
        }
        //PasswordService passwordService=new PasswordService();
        

        public async Task<User> GetUserById(int id) { return await _repository.GetUserById(id); }

        public async Task<User> AddUserToFile(User user)
        {
            Password password= _passwordService.PasswordHardness(user.Password);
            if (password.Level < 3) return null;
            return await _repository.AddUserToFile(user);
        }

        public async Task<User?> Loginto(ExistUser oldUser) { return await _repository.Loginto(oldUser); }

        public IEnumerable<User> GetAllUsers() { return _repository.GetAllUsers(); }

        public async Task<User> UpdateUserDetails(int id, User userToUp) {

            Password password = _passwordService.PasswordHardness(userToUp.Password);
            if (password.Level <3) return null;
            await _repository.UpdateUserDetails(id, userToUp);
            return userToUp;
        }

        
    }
}
