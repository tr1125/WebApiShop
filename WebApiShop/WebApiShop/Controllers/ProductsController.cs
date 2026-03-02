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
        public ProductsController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDTO>>> GetProducts([FromQuery] int position = 1, [FromQuery] int skip = 10,
            [FromQuery] double? minPrice = null, [FromQuery] double? maxPrice = null,
            [FromQuery] string? name = null,
            [FromQuery] string? desc = null, [FromQuery] int?[] categoryIds = null, [FromQuery] string? color = null)
        {
            (List<ProductDTO> products, int total) = await _service.GetProductsByConditions(position, skip, minPrice, maxPrice, name, desc, categoryIds ?? new int?[0], color);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id)
        {
            ProductDTO? product = await _service.GetProductById(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // Admin only
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Post([FromBody] ProductDTO product)
        {
            try
            {
                if (product == null)
                    return BadRequest("Product data is null");
                
                if (string.IsNullOrEmpty(product.ProductName))
                    return BadRequest("ProductName is required");
                
                
                ProductDTO product2 = await _service.AddProduct(product);
                if (product2 == null) return BadRequest();
                return CreatedAtAction(nameof(GetProductById), new { id = product2.ProductId }, product2);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // Admin only
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDTO>> Put(int id, [FromBody] ProductDTO product)
        {
            try
            {
                Console.WriteLine($"PUT request received for id: {id}");
                Console.WriteLine($"Product data: {System.Text.Json.JsonSerializer.Serialize(product)}");
                
                if (product == null)
                    return BadRequest("Product data is null");
                
                if (string.IsNullOrEmpty(product.ProductName))
                    return BadRequest("ProductName is required");
                
                ProductDTO updated = await _service.UpdateProduct(id, product);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PUT: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // Admin only
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {

            // TODO: Add authorization check for Admin
            bool deleted = await _service.DeleteProduct(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
