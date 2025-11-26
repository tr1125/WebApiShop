using Entities;
using Repositories;
using Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiShop.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        public UsersController(IUserService service)
        {
            _service = service;
        }

        public List<Users> Get()
        {
            return _service.GetAllUsers();
        }

        [HttpGet("{id}")]
        public ActionResult<Users> Get(int id)
        {
            Users user = _service.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
             //write in c# code
        

        // POST api/<UsersController>
        [HttpPost]
        public ActionResult<Users> Post([FromBody] Users user)
        {
            Users user2= _service.AddUserToFile(user);
            if (user2 == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = user2.UserId }, user2);
        }

        [HttpPost("login")]
        public ActionResult<Users> Login([FromBody] ExistUser oldUser)
        {
            Users user = _service.Loginto(oldUser);
            if (user == null)
                return Unauthorized();
            return Ok(user);
        }
            
        

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Users userToUpdate)
        {
            Users userres= _service.UpdateUserDetails(id, userToUpdate);
            if (userres == null) return BadRequest("Password is not strong enough");
            return NoContent();
        }

    }
}
