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

        public async Task<List<User>> Get()
        {
            return await _service.GetAllUsers();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            User user =await _service.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
             //write in c# code
        

        // POST api/<UsersController>
        [HttpPost]
        public async Task<ActionResult<User>> Post([FromBody] User user)
        {
            User user2= await _service.AddUserToFile(user);
            if (user2 == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = user2.UserId }, user2);
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] ExistUser oldUser)
        {
            User user = await _service.Loginto(oldUser);
            if (user == null)
                return Unauthorized();
            return Ok(user);

        }
            
        

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User userToUpdate)
        {
            User userres= await _service.UpdateUserDetails(id, userToUpdate);
            if (userres == null) return BadRequest("Password isn't hard enough");
            return NoContent();
        }

        // DELETE api/<UsersController>/5
       
    }
}
