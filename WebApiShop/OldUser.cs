using System.ComponentModel.DataAnnotations;

namespace WebApiShop.Controllers
{
    public class ExistUser
    {
        [EmailAddress, Required]
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
