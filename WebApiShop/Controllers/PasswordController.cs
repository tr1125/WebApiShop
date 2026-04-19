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
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _service;
        private readonly ILogger<PasswordController> _logger;
        public PasswordController(IPasswordService service, ILogger<PasswordController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<Password> Post([FromBody] string password)
        {
            try
            {
                _logger.LogInformation("PasswordHardness check called");
                if (string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("PasswordHardness called with empty password");
                    return BadRequest("Password cannot be empty");
                }
                Password password1 = _service.PasswordHardness(password);
                _logger.LogInformation("PasswordHardness result: Level={Level}", password1.Level);
                return Ok(password1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PasswordHardness");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
