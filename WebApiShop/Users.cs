using System.ComponentModel.DataAnnotations;

namespace WebApiShop.Controllers
{
    public class Users
    {
        [EmailAddress, Required]
        public string UserName { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public int UserId { get; set; }
        public string Password { get; set; }

    }

}
