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
        private readonly ILogger<UsersController> _logger;
        public UsersController(IUserService service, ILogger<UsersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDTO>>> Get()
        {
            try
            {
                _logger.LogInformation("GetAllUsers called");
                List<UserDTO> users = await _service.GetAllUsers();
                _logger.LogInformation("GetAllUsers returned {Count} users", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllUsers");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> Get(int id)
        {
            try
            {
                _logger.LogInformation("GetUserById called with id={Id}", id);
                UserDTO user = await _service.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found for id={Id}", id);
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserById for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> Post([FromBody] UserRequestDTO user)
        {
            try
            {
                _logger.LogInformation("AddUser called for username={UserName}", user?.UserName);
                if (user == null) return BadRequest("User cannot be null");
                UserDTO user2 = await _service.AddUserToFile(user);
                if (user2 == null)
                {
                    _logger.LogWarning("AddUser failed (password too weak or duplicate) for username={UserName}", user.UserName);
                    return BadRequest("Failed to add user. Password may not be strong enough.");
                }
                _logger.LogInformation("User created with id={Id}, username={UserName}", user2.UserId, user2.UserName);
                return CreatedAtAction(nameof(Get), new { id = user2.UserId }, user2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddUser for username={UserName}", user?.UserName);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginDTO>> Login([FromBody] UserLoginDTO oldUser)
        {
            try
            {
                _logger.LogInformation("Login attempted for username={UserName}", oldUser?.UserName);
                if (oldUser == null) return BadRequest("Login data cannot be null");
                UserDTO user = await _service.Loginto(oldUser);
                if (user == null)
                {
                    _logger.LogWarning("Login failed for username={UserName}", oldUser.UserName);
                    return Unauthorized();
                }
                _logger.LogInformation("Login successful for username={UserName}", oldUser.UserName);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login for username={UserName}", oldUser?.UserName);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserRequestDTO userToUpdate)
        {
            try
            {
                _logger.LogInformation("UpdateUser called for id={Id}", id);
                if (userToUpdate == null) return BadRequest("User data cannot be null");
                UserDTO userres = await _service.UpdateUserDetails(id, userToUpdate);
                if (userres == null)
                {
                    _logger.LogWarning("UpdateUser failed for id={Id} — password not strong enough", id);
                    return BadRequest("Password isn't hard enough");
                }
                _logger.LogInformation("User updated successfully for id={Id}", id);
                return Ok(userres);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateUser for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Admin only
        [HttpPost("{id}/promote")]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            try
            {
                _logger.LogInformation("PromoteToAdmin called for id={Id}", id);
                bool success = await _service.PromoteToAdmin(id);
                if (!success)
                {
                    _logger.LogWarning("PromoteToAdmin: user not found for id={Id}", id);
                    return NotFound();
                }
                _logger.LogInformation("User promoted to admin successfully for id={Id}", id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PromoteToAdmin for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
