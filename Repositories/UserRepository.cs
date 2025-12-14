using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly WebApiShopContext _webApiShopContext;

        public UserRepository(WebApiShopContext webApiShopContext)
        {
            _webApiShopContext= webApiShopContext;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _webApiShopContext.Users.ToListAsync();
        }

        public async Task<User> GetUserById(int id)
        {
            User? user =await _webApiShopContext.Users.FindAsync(id);
            return user;
        }


        public async Task<User> AddUserToFile(User user)
        {
            await _webApiShopContext.Users.AddAsync(user);
            await _webApiShopContext.SaveChangesAsync();
            return user;

        }

        public async Task<User?> Loginto(ExistUser oldUser)
        {
            User? user=await _webApiShopContext.Users.FirstOrDefaultAsync(u => (u.UserName==oldUser.UserName && u.Password == oldUser.Password));
            return user;
        }

        public async Task UpdateUserDetails(int id, User userToUp)
        {
            User? user = await _webApiShopContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user != null)
            {
                user.UserName = userToUp.UserName;
                user.Password = userToUp.Password;
                user.FirstName = userToUp.FirstName;
                user.LastName = userToUp.LastName;
            }
            await _webApiShopContext.SaveChangesAsync();

       
        }


    }
}
