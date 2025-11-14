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



        // GET: api/<UsersController>
        [HttpGet]

        
        //public List<Users> Get()
        //{
        //    List<Users> allUsers = new();
        //    using (StreamReader reader = System.IO.File.OpenText(FILE_PATH))
        //    {
        //        string? currentUserInFile;
        //        while ((currentUserInFile = reader.ReadLine()) != null)
        //        {
        //            Users user = JsonSerializer.Deserialize<Users>(currentUserInFile);
        //            allUsers.Add(user);
        //        }
        //    }
        //    return allUsers;
        //}

        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "user1", "user2" };
        //}

        // GET api/<UsersController>/5
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
            return CreatedAtAction(nameof(Get), new { id = user2.UserId }, user2);
        }

        [HttpPost("login")]
        public ActionResult<Users> Login([FromBody] ExistUser oldUser)
        {


            Users user = service.Loginto(oldUser);
            if (user.UserName == oldUser.UserName && user.Password == oldUser.Password)
                return CreatedAtAction(nameof(Get), new { user.UserId }, user);
            return NoContent();
                
        }
            
        

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Users userToUpdate)
        {
            service.UpdateUserDetails(id, userToUpdate);
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
