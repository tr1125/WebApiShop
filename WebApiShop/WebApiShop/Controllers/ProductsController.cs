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

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id)
        {
            try
            {
                _logger.LogInformation("GetProductById called with id={Id}", id);
                ProductDTO? product = await _service.GetProductById(id);
                if (product == null)
                {
                    _logger.LogWarning("Product not found for id={Id}", id);
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductById for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Admin only
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Post([FromBody] ProductDTO product)
        {
            try
            {
                _logger.LogInformation("AddProduct called with name={Name}", product?.ProductName);
                if (product == null)
                    return BadRequest("Product data is null");

                if (string.IsNullOrEmpty(product.ProductName))
                    return BadRequest("ProductName is required");

                ProductDTO product2 = await _service.AddProduct(product);
                if (product2 == null) return BadRequest();
                _logger.LogInformation("Product created with id={Id}", product2.ProductId);
                return CreatedAtAction(nameof(GetProductById), new { id = product2.ProductId }, product2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddProduct");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Admin only
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDTO>> Put(int id, [FromBody] ProductDTO product)
        {
            try
            {
                _logger.LogInformation("UpdateProduct called for id={Id}, name={Name}", id, product?.ProductName);
                if (product == null)
                    return BadRequest("Product data is null");

                if (string.IsNullOrEmpty(product.ProductName))
                    return BadRequest("ProductName is required");

                ProductDTO updated = await _service.UpdateProduct(id, product);
                if (updated == null)
                {
                    _logger.LogWarning("Product not found for update, id={Id}", id);
                    return NotFound();
                }
                _logger.LogInformation("Product updated successfully for id={Id}", id);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProduct for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Admin only
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("DeleteProduct called for id={Id}", id);
                bool deleted = await _service.DeleteProduct(id);
                if (!deleted)
                {
                    _logger.LogWarning("Product not found for deletion, id={Id}", id);
                    return NotFound();
                }
                _logger.LogInformation("Product deleted successfully for id={Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProduct for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
