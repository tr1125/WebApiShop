using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Repositories;
using Services;
using DTOs;
using StackExchange.Redis;
using System.Text.Json;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductsController> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly int _ttlMinutes;

        public ProductsController(IProductService service, ILogger<ProductsController> logger, IDistributedCache cache, IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _service = service;
            _logger = logger;
            _cache = cache;
            _redis = redis;
            _ttlMinutes = configuration.GetValue<int>("Redis:TTLMinutes");
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

                string cacheKey = $"products_{position}_{skip}_{minPrice}_{maxPrice}_{name}_{desc}_{string.Join(",", categoryIds ?? new int?[0])}_{color}";

                var cachedItems = await _cache.GetStringAsync(cacheKey + "_items");
                var cachedTotal = await _cache.GetStringAsync(cacheKey + "_total");

                if (cachedItems != null && cachedTotal != null)
                {
                    _logger.LogInformation("GetProducts returned from cache");
                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    };
                    return Ok(JsonSerializer.Deserialize<List<ProductDTO>>(cachedItems, jsonOptions));
                }

                (List<ProductDTO> products, int total) = await _service.GetProductsByConditions(position, skip, minPrice, maxPrice, name, desc, categoryIds ?? new int?[0], color);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_ttlMinutes)
                };

                var serializeOptions = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                };

                await _cache.SetStringAsync(cacheKey + "_items", JsonSerializer.Serialize(products, serializeOptions), cacheOptions);
                await _cache.SetStringAsync(cacheKey + "_total", JsonSerializer.Serialize(total), cacheOptions);

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

                string cacheKey = $"product_{id}";

                var cached = await _cache.GetStringAsync(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("GetProductById returned from cache for id={Id}", id);
                    return Ok(JsonSerializer.Deserialize<ProductDTO>(cached));
                }

                ProductDTO? product = await _service.GetProductById(id);
                if (product == null)
                {
                    _logger.LogWarning("Product not found for id={Id}", id);
                    return NotFound();
                }

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_ttlMinutes)
                });

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductById for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Admin only
        [AdminOnly]
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
        [AdminOnly]
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

                await InvalidateProductCaches(id);

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
        [AdminOnly]
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

                await InvalidateProductCaches(id);

                _logger.LogInformation("Product deleted successfully for id={Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProduct for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private async Task InvalidateProductCaches(int id)
        {
            await _cache.RemoveAsync($"product_{id}");

            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: "products_*").ToArray();
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key!);
            }
        }
    }
}
