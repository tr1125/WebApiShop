using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;
using DTOs;
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

        [HttpGet]
        public async Task<List<UserDTO>> Get()
        {
            return await _service.GetAllUsers();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> Get(int id)
        {
            UserDTO user = await _service.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        //write in c# code


        // POST api/<UsersController>
        [HttpPost]
        public async Task<ActionResult<UserDTO>> Post([FromBody] UserDTO user)
        {
            UserDTO user2 = await _service.AddUserToFile(user);
            if (user2 == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = user2.Id }, user2);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginDTO>> Login([FromBody] UserLoginDTO oldUser)
        {
            UserLoginDTO user = await _service.Loginto(oldUser);
            if (user == null)
                return Unauthorized();
            return Ok(user);

        }



        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserDTO userToUpdate)
        {
            UserDTO userres = await _service.UpdateUserDetails(id, userToUpdate);
            if (userres == null) return BadRequest("Password isn't hard enough");
            return NoContent();
        }

        // DELETE api/<UsersController>/5

    }
}
