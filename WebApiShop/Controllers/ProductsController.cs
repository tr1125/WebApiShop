using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;
using DTOs;
namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(IProductService service, ILogger<ProductsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDTO>>> GetProducts([FromQuery] int position = 1, [FromQuery] int skip = 10,
            [FromQuery] double? minPrice = null, [FromQuery] double? maxPrice = null,
            [FromQuery] string? name = null,
            [FromQuery] string? desc = null, [FromQuery] int?[] categoryIds = null, [FromQuery] string? color = null)
        {
            try
            {
                _logger.LogInformation("GetProducts called: position={Position}, skip={Skip}, minPrice={Min}, maxPrice={Max}, name={Name}", position, skip, minPrice, maxPrice, name);
                (List<ProductDTO> products, int total) = await _service.GetProductsByConditions(position, skip, minPrice, maxPrice, name, desc, categoryIds ?? new int?[0], color);
                _logger.LogInformation("GetProducts returned {Count} products (total={Total})", products.Count, total);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProducts");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
