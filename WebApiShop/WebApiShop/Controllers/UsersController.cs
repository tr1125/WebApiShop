using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IWebHostEnvironment _env;
        public UsersController(IUserService service, ILogger<UsersController> logger, IWebHostEnvironment env)
        {
            _service = service;
            _logger = logger;
            _env = env;
        }

        private CookieOptions JwtCookieOptions() => new CookieOptions
        {
            HttpOnly = true,
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Secure   = !_env.IsDevelopment(), // false on HTTP localhost, true in production
            Expires  = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Admin only: get all users
        [AdminOnly]
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

        // Logged-in users: get own profile
        [Authorize]
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
                var result = await _service.AddUserToFile(user);
                if (result == null)
                {
                    _logger.LogWarning("AddUser failed (password too weak or duplicate) for username={UserName}", user.UserName);
                    return BadRequest("Failed to add user. Password may not be strong enough.");
                }
                var (user2, token) = result.Value;
                _logger.LogInformation("User created with id={Id}, username={UserName}", user2.UserId, user2.UserName);
                // Plant the JWT as an HttpOnly cookie so the browser sends it automatically
                Response.Cookies.Append("jwt", token, JwtCookieOptions());
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
                var result = await _service.Loginto(oldUser);
                if (result == null)
                {
                    _logger.LogWarning("Login failed for username={UserName}", oldUser.UserName);
                    return Unauthorized();
                }
                var (user, token) = result.Value;
                _logger.LogInformation("Login successful for username={UserName}", oldUser.UserName);
                // Plant the JWT as an HttpOnly cookie
                Response.Cookies.Append("jwt", token, JwtCookieOptions());
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login for username={UserName}", oldUser?.UserName);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Logged-in users: update own profile
        [Authorize]
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

        // Admin only: promote user to admin
        [AdminOnly]
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

        // Logout: clear the jwt cookie
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt", new CookieOptions { SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None, Secure = !_env.IsDevelopment() });
            return Ok(new { message = "Logged out" });
        }
    }
}
