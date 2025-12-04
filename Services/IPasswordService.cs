using Entities;

namespace Services
{
    public interface IPasswordService
    {
        Password PasswordHardness(string password);
    }
}