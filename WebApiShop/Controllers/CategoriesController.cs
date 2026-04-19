using Entities;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using DTOs;
namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly ILogger<CategoriesController> _logger;
        public CategoriesController(ICategoryService service, ILogger<CategoriesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryDTO>>> Get()
        {
            try
            {
                _logger.LogInformation("GetAllCategories called");
                List<CategoryDTO> categories = await _service.GetAllCategories();
                _logger.LogInformation("GetAllCategories returned {Count} categories", categories.Count);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllCategories");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
