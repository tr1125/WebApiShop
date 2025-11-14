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
        UserService service= new UserService();
        // GET: api/<PasswordController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PasswordController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PasswordController>
        [HttpPost]

        public ActionResult<Password> Post([FromBody] string password)
        {
            Password password1 = service.PasswordHardness(password);
            if (password1.Level < 3) return BadRequest();
            return Ok(password1);
        }

        [HttpPost("check")]
        public Password Post1([FromBody] string password)
        {
            Password password1 = service.PasswordHardness(password);
            return password1;
        }
        // PUT api/<PasswordController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PasswordController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
