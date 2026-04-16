using Entities;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;
        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger;
        }

        public Password PasswordHardness(string password)
        {
            _logger.LogInformation("PasswordHardness called");
            Password password1 = new Password();
            password1.Name = password;
            password1.Level = Zxcvbn.Core.EvaluatePassword(password).Score;
            _logger.LogInformation("PasswordHardness result: Level={Level}", password1.Level);
            return password1;
        }
    }
}
