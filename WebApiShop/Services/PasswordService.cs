using Entities;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PasswordService : IPasswordService
    {
        
        public Password PasswordHardness(string password)
        {
            Password password1 = new Password();
            password1.Name = password;
            password1.Level = Zxcvbn.Core.EvaluatePassword(password).Score;
            return password1;
        }
    }
}
