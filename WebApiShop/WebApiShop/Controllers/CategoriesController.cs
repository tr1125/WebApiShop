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

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CategoryDTO category)
        {
            try
            {
                _logger.LogInformation("AddCategory called with name: {Name}", category?.CategoryName);
                if (category == null)
                {
                    _logger.LogWarning("AddCategory called with null body");
                    return BadRequest("Category cannot be null");
                }

                CategoryDTO category2 = await _service.AddCategory(category);
                if (category2 == null)
                {
                    _logger.LogWarning("AddCategory returned null for name: {Name}", category.CategoryName);
                    return BadRequest("Failed to add category");
                }
                _logger.LogInformation("Category created with id {Id}", category2.CetegoryId);
                return CreatedAtAction(nameof(Get), new { id = category2.CetegoryId }, category2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddCategory");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
