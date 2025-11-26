using Entities;
using System.Text.Json;
using WebApiShop.Controllers;
namespace Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string FILE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "file.txt");

        public List<Users> GetAllUsers()
        {
            List<Users> allUsers = new();
            using (StreamReader reader = System.IO.File.OpenText(FILE_PATH))
            {
                string? currentUserInFile;
                while ((currentUserInFile = reader.ReadLine()) != null)
                {
                    Users user = JsonSerializer.Deserialize<Users>(currentUserInFile);
                    allUsers.Add(user);
                }
            }
            return allUsers;
        }

        public Users GetUserById(int id)
        {
            using (StreamReader reader = System.IO.File.OpenText(FILE_PATH))
            {
                string? currentUserInFile;
                while ((currentUserInFile = reader.ReadLine()) != null)
                {
                    Users user = JsonSerializer.Deserialize<Users>(currentUserInFile);
                    return user;
                }
            }
            return null;
        }


        public Users addUserToFile(Users user)
        {
            int numberOfUsers = System.IO.File.ReadLines(FILE_PATH).Count();
            user.UserId = numberOfUsers + 1;
            string userJson = JsonSerializer.Serialize(user);
            System.IO.File.AppendAllText(FILE_PATH, userJson + Environment.NewLine);
            return user;

        }

        public Users Loginto(ExistUser oldUser)
        {
            using (StreamReader reader = System.IO.File.OpenText(FILE_PATH))
            {
                string? currentUserInFile;
                while ((currentUserInFile = reader.ReadLine()) != null)
                {
                    Users user = JsonSerializer.Deserialize<Users>(currentUserInFile);
                    if (user.UserName == oldUser.UserName && user.Password == oldUser.Password)
                        return user;
                    
                }

            }
            return null;
        }

        public void UpdateUserDetails(int id, Users userToUp)
        {
            string textToReplace = string.Empty;
            using (StreamReader reader = System.IO.File.OpenText(FILE_PATH))
            {
                string currentUserInFile;
                while ((currentUserInFile = reader.ReadLine()) != null)
                {
                    Users user = JsonSerializer.Deserialize<Users>(currentUserInFile);
                    if (user.UserId == id)
                        textToReplace = currentUserInFile;
                }
            }

            if (textToReplace != string.Empty)
            {
                string text = System.IO.File.ReadAllText(FILE_PATH);
                text = text.Replace(textToReplace, JsonSerializer.Serialize(userToUp));
                System.IO.File.WriteAllText(FILE_PATH, text);
            }
        }


    }
}
