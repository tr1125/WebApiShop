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
        UserService service = new UserService();

        public List<Users> Get()
        {
            return service.GetAllUsers();
        }

        [HttpGet("{id}")]
        public ActionResult<Users> Get(int id)
        {
            Users user = service.GetUserById(id);
            if (user.UserId == id)
                    return Ok(user);
            return NoContent();
        }
             //write in c# code
        

        // POST api/<UsersController>
        [HttpPost]
        public ActionResult<Users> Post([FromBody] Users user)
        {
            Users user2= service.AddUserToFile(user);
            if (user2 == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = user2.UserId }, user2);
        }

        [HttpPost("login")]
        public ActionResult<Users> Login([FromBody] ExistUser oldUser)
        {
            Users user = service.Loginto(oldUser);
            if (user == null) return BadRequest();
            if (user.UserName == oldUser.UserName && user.Password == oldUser.Password)
                return CreatedAtAction(nameof(Get), new { user.UserId }, user);
            return NoContent();
                
        }
            
        

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public ActionResult<Users> Put(int id, [FromBody] Users userToUpdate)
        {
            Users userres=service.UpdateUserDetails(id, userToUpdate);
            if (userres == null) return BadRequest();
            return Ok(userres);
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
