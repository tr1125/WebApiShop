using Entities;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IUserService _service;
        public PasswordController(IUserService service)
        {
            _service = service;
        }
        // POST api/<PasswordController>
        [HttpPost]
        public Password Post([FromBody] string password)
        {
            Password password1 = _service.PasswordHardness(password);
            return password1;
        }
    }
}
